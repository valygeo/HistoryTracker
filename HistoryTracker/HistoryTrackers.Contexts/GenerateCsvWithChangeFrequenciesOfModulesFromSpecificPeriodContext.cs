
using Domain;
using Domain.MetaData;
using HistoryTracker.Contexts.Base;
using System.Text.RegularExpressions;
using System.Text;

namespace HistoryTracker.Contexts
{
    public class GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodContext
    {
        private readonly IGenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodGateway _gateway;
        private readonly CreateAllTimeLogFileContext _createLogFileContext;
        private readonly ExtractCommitsForSpecifiedPeriodFromLogFileContext _extractCommitsForSpecifiedPeriodContext;

        public GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodContext(IGenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodGateway gateway, CreateAllTimeLogFileContext createLogFileContext,
              ExtractCommitsForSpecifiedPeriodFromLogFileContext extractCommitsForSpecifiedPeriodContext)
        {
            _gateway = gateway;
            _createLogFileContext = createLogFileContext;
            _extractCommitsForSpecifiedPeriodContext = extractCommitsForSpecifiedPeriodContext;
        }
        public GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodResponse Execute(CreateLogFileFromSpecifiedPeriodData request)
        {
            var createLogFileResponse = _createLogFileContext.Execute(request.clonedRepositoryPath);
            if (createLogFileResponse.IsSuccess)
            {
                    var formattedPeriodEndDate = $"{request.periodEndDate:yyyy-MM-dd}";
                    var csvFileName = $"{request.repositoryName}_change_frequencies_of_modules_before_{formattedPeriodEndDate}.csv";
                    var csvFilePath = Path.Combine(request.clonedRepositoryPath, csvFileName);
                    var extractCommits = _extractCommitsForSpecifiedPeriodContext.Execute(createLogFileResponse.LogFilePath, formattedPeriodEndDate);
                    var revisionsOfModules = GetChangeFrequenciesAndAuthors(extractCommits);
                    var createCsvResponse = _gateway.CreateCsvFileWithChangeFrequenciesOfModules(revisionsOfModules, csvFilePath);
                    if (createCsvResponse)
                        return new GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodResponse { IsSuccess = true, GeneratedCsvPath = csvFilePath };
                    return new GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodResponse { IsSuccess = false, Error = "Error trying to create csv file!" };
                    
            }
            return new GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodResponse { IsSuccess = false, Error = createLogFileResponse.Error };
        }
        private ICollection<ChangeFrequency> GetChangeFrequenciesAndAuthors(ExtractAllCommitsForSpecifiedPeriodResponse commits)
        {
            var modulesWithChangeFrequenciesAndAuthors = new List<ChangeFrequency>();

            foreach (var commit in commits.CommitsBeforeEndDate)
            {
                foreach (var commitDetail in commit.CommitDetails)
                {
                    var pathWithWindowsSlashes = commitDetail.EntityChangedName.Replace("/", "\\");
                    var pathWithoutComma = pathWithWindowsSlashes.Replace(",", "");
                    var relativePath = $".\\{pathWithoutComma}";

                    if (IsThereARenameOrMove(relativePath))
                    {
                        var initialAndNewIntegratedValue = ProcessRenameOrMovePattern(relativePath);
                        relativePath = initialAndNewIntegratedValue[1];
                        foreach (var module in modulesWithChangeFrequenciesAndAuthors)
                        {
                            if (module.EntityPath == initialAndNewIntegratedValue[0])
                            {
                                module.EntityPath = relativePath;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(relativePath))
                    {
                        var existingEntity = modulesWithChangeFrequenciesAndAuthors.FirstOrDefault(e => e.EntityPath.Equals(relativePath));
                        if (existingEntity == null)
                        {
                            var entity = new ChangeFrequency
                            {
                                EntityPath = relativePath,
                                Revisions = 1,
                                Authors = commit.Author
                            };
                            modulesWithChangeFrequenciesAndAuthors.Add(entity);
                        }
                        else
                        {
                            existingEntity.Revisions++;
                            var stringBuilder = new StringBuilder(existingEntity.Authors);
                            if (!existingEntity.Authors.Contains(commit.Author))
                            {
                                stringBuilder.Append(';').Append(commit.Author);
                                existingEntity.Authors = stringBuilder.ToString();
                            }
                        }
                    }
                }
            }
            foreach (var commit in commits.CommitsAfterEndDate.Reverse())
            {
                foreach (var commitDetail in commit.CommitDetails)
                {
                    var pathWithWindowsSlashes = commitDetail.EntityChangedName.Replace("/", "\\");
                    var pathWithoutComma = pathWithWindowsSlashes.Replace(",", "");
                    var relativePath = $".\\{pathWithoutComma}";
                    if (IsThereARenameOrMove(relativePath))
                    {
                        var initialAndNewIntegratedValue = ProcessRenameOrMovePattern(relativePath);
                        relativePath = initialAndNewIntegratedValue[1];
                        foreach (var module in modulesWithChangeFrequenciesAndAuthors)
                        {
                            if (module.EntityPath == initialAndNewIntegratedValue[0])
                            {
                                module.EntityPath = relativePath;
                            }
                        }
                    }
                }
            }
            return SortInDescendingOrder(modulesWithChangeFrequenciesAndAuthors);
        }
        private static string[] ProcessRenameOrMovePattern(string value)
        {
            Regex renameOrMovePattern = new Regex(@"\\{+[^{}]*[\s]=>[\s][^{}]*[\s]*\}");

            var matches = renameOrMovePattern.Matches(value);

            if (matches.Count > 0)
            {
                var names = matches[0].Value.Replace("\\{", "").Replace("}", "").Split(new[] { " => " }, StringSplitOptions.None);
                var initial = names[0];
                var @new = names[1];

                var initialIntegrated = value.Replace(matches[0].Value, string.IsNullOrEmpty(initial) ? "" : "\\" + initial);
                var newIntegrated = value.Replace(matches[0].Value, string.IsNullOrEmpty(@new) ? "" : "\\" + @new);

                return new[] { initialIntegrated, newIntegrated };
            }
            return new string[] { };
        }

        private static bool IsThereARenameOrMove(string result)
        {
            if (string.IsNullOrEmpty(result))
                return false;

            return result.IndexOf("{", StringComparison.InvariantCulture) > -1;
        }
        private static ICollection<ChangeFrequency> SortInDescendingOrder(
            ICollection<ChangeFrequency> changeFrequenciesAndAuthorsOfModules)
        {
            return changeFrequenciesAndAuthorsOfModules.OrderByDescending(module => module.Revisions).ToList();
        }
    }

    public class GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodResponse : BaseResponse
    {
        public string GeneratedCsvPath { get; set; }
    }
}
