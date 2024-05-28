
using System.Text.RegularExpressions;
using Domain;
using Domain.MetaData;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class GenerateCsvWithChangeFrequenciesOfAllModulesContext
    {
        private readonly IGenerateCsvWithChangeFrequenciesOfAllModulesGateway _gateway;
        private readonly CreateAllTimeLogFileContext _createLogFileContext;
        private readonly ExtractCommitsContext _extractAllCommitsContext;

        public GenerateCsvWithChangeFrequenciesOfAllModulesContext(IGenerateCsvWithChangeFrequenciesOfAllModulesGateway gateway,
            CreateAllTimeLogFileContext createLogFileContext,
            ExtractCommitsContext extractAllCommitsContext)
        {
            _gateway = gateway;
            _createLogFileContext = createLogFileContext;
            _extractAllCommitsContext = extractAllCommitsContext;
        }

        public GenerateCsvWithChangeFrequenciesOfAllModulesResponse Execute(string clonedRepositoryPath)
        {
            var repositoryName = Path.GetFileNameWithoutExtension(clonedRepositoryPath);
            var csvFileName = $"{repositoryName}_change_frequencies_of_modules.csv";
            var csvFilePath = Path.Combine(clonedRepositoryPath, csvFileName);

            var createLogFileResponse = _createLogFileContext.Execute(clonedRepositoryPath);
            if (createLogFileResponse.IsSuccess)
            {
                var extractAllCommitsResponse = _extractAllCommitsContext.Execute(createLogFileResponse.LogFilePath);
                var revisionsOfModules = GetChangeFrequencies(extractAllCommitsResponse); 
                var createCsvResponse =  _gateway.CreateCsvFileWithChangeFrequenciesOfModules(revisionsOfModules, csvFilePath);
                if(createCsvResponse)
                    return new GenerateCsvWithChangeFrequenciesOfAllModulesResponse { IsSuccess = true, GeneratedCsvPath = csvFilePath};
                return new GenerateCsvWithChangeFrequenciesOfAllModulesResponse { IsSuccess = false, Error = "Error trying to create csv file!" };
            }
            return new GenerateCsvWithChangeFrequenciesOfAllModulesResponse { IsSuccess = false, Error = createLogFileResponse.Error };
        }
        
        private Dictionary<string,int> GetChangeFrequencies(ExtractCommitsResponse commits)
        {
            var modulesWithChangeFrequencies = new Dictionary<string, int>();
            
            foreach (var commitEntry in commits.Commits)
            {
                var commit = commitEntry.Value;
                foreach (var commitDetailEntry in commit.CommitDetails)
                {
                    var pathWithWindowsSlashes = commitDetailEntry.Key.Replace("/", "\\");
                    var pathWithoutComma = pathWithWindowsSlashes.Replace(",", "");
                    var relativePath = $".\\{pathWithoutComma}";

                    if (IsThereARenameOrMove(relativePath))
                    {
                       var initialAndNewIntegratedValue =  ProcessRenameOrMovePattern(relativePath);
                       relativePath = initialAndNewIntegratedValue[1];
                       if (modulesWithChangeFrequencies.ContainsKey(initialAndNewIntegratedValue[0]))
                       {
                           var moduleChangeFrequency = modulesWithChangeFrequencies[initialAndNewIntegratedValue[0]];
                           modulesWithChangeFrequencies.Remove(initialAndNewIntegratedValue[0]);
                           modulesWithChangeFrequencies[relativePath] = moduleChangeFrequency;
                       }
                    }

                    if (!string.IsNullOrWhiteSpace(relativePath))
                    {
                        var existingEntity = modulesWithChangeFrequencies.ContainsKey(relativePath);
                        if (!existingEntity)
                        {
                            var entity = new KeyValuePair<string, int>(relativePath, 1);
                            modulesWithChangeFrequencies.Add(entity.Key, entity.Value);
                        }
                        else
                        {
                            modulesWithChangeFrequencies[relativePath]++;
                        }
                    }
                }
            }
            return SortInDescendingOrder(modulesWithChangeFrequencies);
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
        private static Dictionary<string,int> SortInDescendingOrder(
            Dictionary<string,int> changeFrequenciesAndAuthorsOfModules)
        {
            return changeFrequenciesAndAuthorsOfModules.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public class GenerateCsvWithChangeFrequenciesOfAllModulesResponse : BaseResponse
        {
            public string GeneratedCsvPath { get; set; }
        }
    }
}
