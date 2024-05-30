
using Domain;
using Domain.MetaData;
using HistoryTracker.Contexts.Base;
using System.Text.RegularExpressions;


namespace HistoryTracker.Contexts
{
    public class GenerateCsvWithMainAuthorsPerModuleContext
    {
        private readonly IGenerateCsvWithMainAuthorsPerModuleGateway _gateway;
        private readonly CreateAllTimeLogFileContext _createAllTimeLogFileContext;
        private readonly ExtractCommitsForSpecifiedPeriodFromLogFileContext _extractCommitsForSpecifiedPeriodContext;

        public GenerateCsvWithMainAuthorsPerModuleContext(IGenerateCsvWithMainAuthorsPerModuleGateway gateway, CreateAllTimeLogFileContext createAllTimeLogFileContext, ExtractCommitsForSpecifiedPeriodFromLogFileContext extractCommitsForSpecifiedPeriodContext)
        {
            _gateway = gateway;
            _createAllTimeLogFileContext = createAllTimeLogFileContext;
            _extractCommitsForSpecifiedPeriodContext = extractCommitsForSpecifiedPeriodContext;
        }

        public GenerateCsvWithMainAuthorPerModuleResponse Execute(CreateCsvFromSpecifiedPeriodRequest request)
        {
            var createLogFileResponse = _createAllTimeLogFileContext.Execute(request.ClonedRepositoryPath);
            if (createLogFileResponse.IsSuccess)
            {
                var formattedPeriodEndDate = $"{request.PeriodEndDate:yyyy-MM-dd}";
                var csvFileName = $"{request.RepositoryName}_main_authors_per_modules_before_{formattedPeriodEndDate}.csv";
                var csvFilePath = Path.Combine(request.ClonedRepositoryPath, csvFileName);
                var extractCommitsResponse = _extractCommitsForSpecifiedPeriodContext.Execute(createLogFileResponse.LogFilePath, formattedPeriodEndDate);
                if (extractCommitsResponse.IsSuccess)
                {
                    var revisionsOfModules = GetChangeFrequenciesAndMainAuthors(extractCommitsResponse);
                    var createCsvResponse = _gateway.CreateCsvFileWithMainAuthorsAndChangeFrequenciesOfModules(revisionsOfModules, csvFilePath);
                    if (createCsvResponse)
                        return new GenerateCsvWithMainAuthorPerModuleResponse { IsSuccess = true, GeneratedCsvPath = csvFilePath };
                    return new GenerateCsvWithMainAuthorPerModuleResponse { IsSuccess = false, Error = "Error trying to create csv file!" };
                }
                return new GenerateCsvWithMainAuthorPerModuleResponse { IsSuccess = false, Error = extractCommitsResponse.Error };
            }
            return new GenerateCsvWithMainAuthorPerModuleResponse { IsSuccess = false, Error = createLogFileResponse.Error };
            
        }
          
        private Dictionary<string, FileMainAuthor> GetChangeFrequenciesAndMainAuthors(ExtractAllCommitsForSpecifiedPeriodResponse commits)
        {
            var modulesWithChangeFrequenciesAndAuthors = new Dictionary<string, List<FileMainAuthor>>();
            foreach (var commitEntry in commits.CommitsBeforeEndDate)
            {
                var commit = commitEntry.Value;
                foreach (var commitDetail in commit.CommitDetails)
                {
                    var pathWithWindowsSlashes = commitDetail.Key.Replace("/", "\\");
                    var pathWithoutComma = pathWithWindowsSlashes.Replace(",", "");
                    var relativePath = $".\\{pathWithoutComma}";

                    if (IsThereARenameOrMove(relativePath))
                    {
                        var initialAndNewIntegratedValue = ProcessRenameOrMovePattern(relativePath);
                        relativePath = initialAndNewIntegratedValue[1];
                        if (modulesWithChangeFrequenciesAndAuthors.ContainsKey(initialAndNewIntegratedValue[0]))
                        {
                            var fileMainAuthorAndChangeFrequency = modulesWithChangeFrequenciesAndAuthors[initialAndNewIntegratedValue[0]];
                            modulesWithChangeFrequenciesAndAuthors.Remove(initialAndNewIntegratedValue[0]);
                            modulesWithChangeFrequenciesAndAuthors[relativePath] = fileMainAuthorAndChangeFrequency;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(relativePath))
                    {
                        var fileMainAuthors = new List<FileMainAuthor>();
                        var existingModule = modulesWithChangeFrequenciesAndAuthors.ContainsKey(relativePath);
                        if (!existingModule)
                        {   
                            fileMainAuthors.Add(new FileMainAuthor
                            {
                                MainAuthor = commit.Author,
                                Revisions = 1
                            });
                            modulesWithChangeFrequenciesAndAuthors.Add(relativePath, fileMainAuthors);
                        }
                        else
                        {
                            var fileAuthorIfAlreadyExist = modulesWithChangeFrequenciesAndAuthors[relativePath].FirstOrDefault(author => author.MainAuthor.Equals(commit.Author));

                            if (fileAuthorIfAlreadyExist != null)
                                fileAuthorIfAlreadyExist.Revisions++;
                            else
                            {
                                modulesWithChangeFrequenciesAndAuthors[relativePath].Add(new FileMainAuthor
                                {
                                    MainAuthor = commit.Author,
                                    Revisions = 1
                                });
                            }
                        }
                    }
                }
            }
            foreach (var commitEntry in commits.CommitsAfterEndDate.Reverse())
            {
                var commit = commitEntry.Value;
                foreach (var commitDetail in commit.CommitDetails)
                {
                    var pathWithWindowsSlashes = commitDetail.Key.Replace("/", "\\");
                    var pathWithoutComma = pathWithWindowsSlashes.Replace(",", "");
                    var relativePath = $".\\{pathWithoutComma}";

                    if (IsThereARenameOrMove(relativePath))
                    {
                        var initialAndNewIntegratedValue = ProcessRenameOrMovePattern(relativePath);
                        relativePath = initialAndNewIntegratedValue[1];
                        if (modulesWithChangeFrequenciesAndAuthors.ContainsKey(initialAndNewIntegratedValue[0]))
                        {
                            var fileMainAuthorAndChangeFrequency = modulesWithChangeFrequenciesAndAuthors[initialAndNewIntegratedValue[0]];
                            modulesWithChangeFrequenciesAndAuthors.Remove(initialAndNewIntegratedValue[0]);
                            modulesWithChangeFrequenciesAndAuthors[relativePath] = fileMainAuthorAndChangeFrequency;
                        }
                    }
                }
            }
            return FindMainAuthorsPerModuleByNumberOfRevisions(modulesWithChangeFrequenciesAndAuthors);
        }

        private static Dictionary<string, FileMainAuthor> FindMainAuthorsPerModuleByNumberOfRevisions(Dictionary<string, List<FileMainAuthor>> mainAuthorsAndChangeFrequenciesPerModule)
        {
            var mainAuthorsByMaxRevisions = new Dictionary<string, FileMainAuthor>();
            foreach (var authorEntry in mainAuthorsAndChangeFrequenciesPerModule)
            {
                var modulePath = authorEntry.Key;
                var authorsList = authorEntry.Value;
                var fileMainAuthorByRevisions = authorsList.OrderByDescending(a => a.Revisions).First();
                mainAuthorsByMaxRevisions.Add(modulePath, new FileMainAuthor
                {
                    MainAuthor = fileMainAuthorByRevisions.MainAuthor,
                    Revisions = fileMainAuthorByRevisions.Revisions
                });
            }
            return mainAuthorsByMaxRevisions.OrderByDescending(kv => kv.Value.Revisions)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
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
    }

    public class GenerateCsvWithMainAuthorPerModuleResponse : BaseResponse
    {
        public string GeneratedCsvPath { get; set; }
    }
}
