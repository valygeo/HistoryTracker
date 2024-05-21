
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
          
        private ICollection<FileMainAuthor> GetChangeFrequenciesAndMainAuthors(ExtractAllCommitsForSpecifiedPeriodResponse commits)
        {
            var modulesWithChangeFrequenciesAndAuthors = new List<FileMainAuthor>();
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
                        var existingEntity = modulesWithChangeFrequenciesAndAuthors.FirstOrDefault(e => e.EntityPath.Equals(relativePath) && e.MainAuthor.Equals(commit.Author));
                        if (existingEntity == null)
                        {
                            var entity = new FileMainAuthor
                            {
                                EntityPath = relativePath,
                                MainAuthor = commit.Author,
                                Revisions = 1,
                            };
                            modulesWithChangeFrequenciesAndAuthors.Add(entity);
                        }
                        else
                        {
                            existingEntity.Revisions++;
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
            return FindMainAuthorsPerModuleByNumberOfRevisions(modulesWithChangeFrequenciesAndAuthors);
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
        private static ICollection<FileMainAuthor> FindMainAuthorsPerModuleByNumberOfRevisions(
            ICollection<FileMainAuthor> mainAuthorsAndChangeFrequenciesPerModule)
        {
            var mainAuthorsByMaxRevisions = new List<FileMainAuthor>();

            foreach (var mainAuthor in mainAuthorsAndChangeFrequenciesPerModule)
            {
                var existingEntity = mainAuthorsByMaxRevisions.FirstOrDefault(e => e.EntityPath == mainAuthor.EntityPath);

                if (existingEntity == null)
                {
                    mainAuthorsByMaxRevisions.Add(mainAuthor);
                }
                else
                {
                    if (mainAuthor.Revisions > existingEntity.Revisions)
                    {
                        existingEntity.Revisions = mainAuthor.Revisions;
                        existingEntity.MainAuthor = mainAuthor.MainAuthor;
                    }
                }
            }
            return mainAuthorsByMaxRevisions.OrderByDescending(m => m.Revisions).ToList();
        }
        //private static IDictionary<string, FileMainAuthor> FindMainAuthorsPerModuleByNumberOfRevisions(
        //    IDictionary<string, FileMainAuthor> mainAuthorsAndChangeFrequenciesPerModule)
        //{
        //    var mainAuthorsByMaxRevisions = new Dictionary<string, FileMainAuthor>();
        //    foreach (var entry in mainAuthorsAndChangeFrequenciesPerModule)
        //    {
        //        if(!mainAuthorsByMaxRevisions.ContainsKey(entry.Key))
        //            mainAuthorsByMaxRevisions[entry.Key] = entry.Value;
        //        else
        //        {
        //            if (entry.Value.Revisions > mainAuthorsByMaxRevisions[entry.Key].Revisions)
        //            {
        //                mainAuthorsByMaxRevisions[entry.Key].Revisions = entry.Value.Revisions;
        //                mainAuthorsByMaxRevisions[entry.Key].MainAuthor = entry.Value.MainAuthor;
        //            }
        //        }
        //    }
        //    return mainAuthorsByMaxRevisions;
        //}
    }

    public class GenerateCsvWithMainAuthorPerModuleResponse : BaseResponse
    {
        public string GeneratedCsvPath { get; set; }
    }
}
