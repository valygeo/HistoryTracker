
using Domain;
using HistoryTracker.Contexts.Base;
using Domain.MetaData;

namespace HistoryTracker.Contexts
{
    public class MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodContext
    {
        private readonly CloneRepositoryContext _cloneRepositoryContext;
        private readonly GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodContext _generateCsvWithChangeFrequenciesOfModules;
        private readonly GenerateCsvWithNumberOfCodeLinesContext _generateCsvWithNumberOfCodeLines;
        private readonly IMergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodGateway _gateway;

        public MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodContext(CloneRepositoryContext cloneRepositoryContext, GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodContext generateCsvWithChangeFrequenciesOfModules,
            GenerateCsvWithNumberOfCodeLinesContext generateCsvWithNumberOfCodeLinesContext, IMergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodGateway gateway)
        {
            _cloneRepositoryContext = cloneRepositoryContext;
            _generateCsvWithChangeFrequenciesOfModules = generateCsvWithChangeFrequenciesOfModules;
            _generateCsvWithNumberOfCodeLines = generateCsvWithNumberOfCodeLinesContext;
            _gateway = gateway;
        }

        public MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodResponse Execute(ComplexityMetricsRequest request, string clocPath)
        {
            if (!String.IsNullOrWhiteSpace(request.RepositoryUrl))
            {
                var cloneRepositoryResponse = _cloneRepositoryContext.Execute(request.RepositoryUrl);

                if (cloneRepositoryResponse.IsSuccess)
                {
                    var repositoryName = Path.GetFileNameWithoutExtension(cloneRepositoryResponse.ClonedRepositoryPath);
                    
                    var generateCsvWithChangeFrequenciesResponse =
                        _generateCsvWithChangeFrequenciesOfModules.Execute(new CreateCsvFromSpecifiedPeriodRequest
                        {
                            ClonedRepositoryPath = cloneRepositoryResponse.ClonedRepositoryPath,
                            RepositoryName = repositoryName,
                            PeriodEndDate = request.EndDatePeriod,
                        });

                    var generateCsvWithNumberOfCodeLinesResponse = _generateCsvWithNumberOfCodeLines.Execute(cloneRepositoryResponse.ClonedRepositoryPath, clocPath);
                    if (generateCsvWithChangeFrequenciesResponse.IsSuccess && generateCsvWithNumberOfCodeLinesResponse.IsSuccess)
                    {
                        var csvFileName = $"{repositoryName}_complexity_metrics_before_{request.EndDatePeriod:yyyy-MM-dd}.csv";
                        var csvFilePath = Path.Combine(cloneRepositoryResponse.ClonedRepositoryPath, csvFileName);
                        var generateCsvResponse = GenerateMergedCsv(generateCsvWithChangeFrequenciesResponse.GeneratedCsvPath, generateCsvWithNumberOfCodeLinesResponse.GeneratedCsvPath, csvFilePath);
                        if (generateCsvResponse)
                            return new MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodResponse
                            { IsSuccess = true, MergedCsvFilePath = csvFilePath };
                        return new MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodResponse
                        { IsSuccess = false, Error = "Error trying to generate the csv file!" };
                    }

                    if (!generateCsvWithChangeFrequenciesResponse.IsSuccess)
                        return new MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodResponse { IsSuccess = false, Error = generateCsvWithChangeFrequenciesResponse.Error };
                    if (!generateCsvWithNumberOfCodeLinesResponse.IsSuccess)
                        return new MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodResponse { IsSuccess = false, Error = generateCsvWithNumberOfCodeLinesResponse.Error };
                }
                return new MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodResponse
                { IsSuccess = false, Error = cloneRepositoryResponse.Error };
            }
            return new MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodResponse
            { IsSuccess = false, Error = "Repository url is empty!" };
        }
        private bool GenerateMergedCsv(string changeFrequenciesCsvPath, string numberOfCodeLinesCsvPath, string csvFilePath)
        {
            var changeFrequenciesFile = File.ReadAllLines(changeFrequenciesCsvPath);
            var numberOfCodeLinesFile = File.ReadAllLines(numberOfCodeLinesCsvPath);
            var changeFrequenciesMetrics = new Dictionary<string, int>();
            var numberOfCodeLinesMetrics = new Dictionary<string, int>();
            var mergedMetrics = new Dictionary<string, ChangeFrequencyAndCodeMetric>();

            for (int i = 1; i < changeFrequenciesFile.Length; i++)
            {
                var parts = changeFrequenciesFile[i].Split(',', 2);
                changeFrequenciesMetrics.Add(parts[0], int.Parse(parts[1]));
                // parts[0] = modulePath ; parts[1] = changeFrequency
            }

            for (int i = 1; i < numberOfCodeLinesFile.Length - 1; i++)
            {
                var parts = numberOfCodeLinesFile[i].Split(",", 5);
                numberOfCodeLinesMetrics.Add(parts[1], int.Parse(parts[4]));
                //parts[1] = module path ; parts[4] = numberOfCodeLines
            }

            foreach (var codeMetric in numberOfCodeLinesMetrics)
            {
                if (changeFrequenciesMetrics.ContainsKey(codeMetric.Key))
                {
                    mergedMetrics.Add(codeMetric.Key, new ChangeFrequencyAndCodeMetric
                    {
                        CodeLines = codeMetric.Value,
                        Revisions = changeFrequenciesMetrics[codeMetric.Key]
                    });
                }
                else
                {
                    mergedMetrics.Add(codeMetric.Key, new ChangeFrequencyAndCodeMetric
                    {
                        CodeLines = codeMetric.Value,
                        Revisions = 0
                    });
                }
            }
            var sortedMetrics = SortDescendingAfterChangeFrequencyAndCodeSize(mergedMetrics);
            var response = _gateway.CreateCsvFileWithChangeFrequencyAndNumberOfCodeLines(sortedMetrics, csvFilePath);
            return response;
        }

        public Dictionary<string, ChangeFrequencyAndCodeMetric> SortDescendingAfterChangeFrequencyAndCodeSize(Dictionary<string, ChangeFrequencyAndCodeMetric> metrics)
        {
            return metrics.OrderByDescending(kv => kv.Value.Revisions)
                .ThenByDescending(kv => kv.Value.CodeLines)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

    }

    public class MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodResponse : BaseResponse
    {
        public string MergedCsvFilePath { get; set; }
    }
}
