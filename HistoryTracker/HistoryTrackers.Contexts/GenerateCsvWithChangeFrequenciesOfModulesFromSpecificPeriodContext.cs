
using Domain;
using Domain.MetaData;
using HistoryTracker.Contexts.Base;
using System.Text.RegularExpressions;


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
        public GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodResponse Execute(CreateCsvFromSpecifiedPeriodRequest request)
        {
            var createLogFileResponse = _createLogFileContext.Execute(request.ClonedRepositoryPath);
            if (createLogFileResponse.IsSuccess)
            {
                var formattedPeriodEndDate = $"{request.PeriodEndDate:yyyy-MM-dd}";
                var csvFileName = $"{request.RepositoryName}_change_frequencies_of_modules_before_{formattedPeriodEndDate}.csv";
                var csvFilePath = Path.Combine(request.ClonedRepositoryPath, csvFileName);

                var extractCommitsResponse = _extractCommitsForSpecifiedPeriodContext.Execute(createLogFileResponse.LogFilePath, formattedPeriodEndDate);
                if (extractCommitsResponse.IsSuccess)
                {
                    var revisionsOfModules = GetChangeFrequencies(extractCommitsResponse);
                    var createCsvResponse = _gateway.CreateCsvFileWithChangeFrequenciesOfModules(revisionsOfModules, csvFilePath);
                    if (createCsvResponse)
                        return new GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodResponse { IsSuccess = true, GeneratedCsvPath = csvFilePath };
                    return new GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodResponse { IsSuccess = false, Error = "Error trying to create csv file!" };
                }
                return new GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodResponse
                    { IsSuccess = false, Error = extractCommitsResponse.Error };
            }
            return new GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodResponse { IsSuccess = false, Error = createLogFileResponse.Error };
        }
        private Dictionary<string,int> GetChangeFrequencies(ExtractAllCommitsForSpecifiedPeriodResponse commits)
        {
            var modulesWithChangeFrequencies = new Dictionary<string, int>();

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
                        if (modulesWithChangeFrequencies.ContainsKey(initialAndNewIntegratedValue[0]))
                        {
                            var moduleChangeFrequency = modulesWithChangeFrequencies[initialAndNewIntegratedValue[0]];
                            modulesWithChangeFrequencies.Remove(initialAndNewIntegratedValue[0]);
                            modulesWithChangeFrequencies[relativePath] = moduleChangeFrequency;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(relativePath))
                    {
                        var existingModule = modulesWithChangeFrequencies.ContainsKey(relativePath);
                        if (!existingModule)
                            modulesWithChangeFrequencies.Add(relativePath,1);
                        modulesWithChangeFrequencies[relativePath]++;
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
                        if (modulesWithChangeFrequencies.ContainsKey(initialAndNewIntegratedValue[0]))
                        {
                            var moduleChangeFrequency = modulesWithChangeFrequencies[initialAndNewIntegratedValue[0]];
                            modulesWithChangeFrequencies.Remove(initialAndNewIntegratedValue[0]);
                            modulesWithChangeFrequencies[relativePath] = moduleChangeFrequency;
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
        private static Dictionary<string, int> SortInDescendingOrder(
            Dictionary<string, int> changeFrequenciesOfModules)
        {
            return changeFrequenciesOfModules.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }

    public class GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodResponse : BaseResponse
    {
        public string GeneratedCsvPath { get; set; }
    }
}
