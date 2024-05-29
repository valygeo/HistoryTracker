
using Domain;
using Domain.MetaData;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class MergeChangeFrequenciesAndNumberOfCodeLinesContext
    {
        private readonly CloneRepositoryContext _cloneRepositoryContext;
        private readonly GenerateCsvWithChangeFrequenciesOfAllModulesContext _changeFrequenciesContext;
        private readonly GenerateCsvWithNumberOfCodeLinesContext _numberOfCodeLinesContext;
        private readonly IMergeChangeFrequenciesAndNumberOfCodeLinesGateway _gateway;

        public MergeChangeFrequenciesAndNumberOfCodeLinesContext(CloneRepositoryContext cloneRepositoryContext, GenerateCsvWithChangeFrequenciesOfAllModulesContext changeFrequenciesContext, GenerateCsvWithNumberOfCodeLinesContext numberOfCodeLinesContext, IMergeChangeFrequenciesAndNumberOfCodeLinesGateway gateway)
        {
            _cloneRepositoryContext = cloneRepositoryContext;
            _changeFrequenciesContext = changeFrequenciesContext;
            _numberOfCodeLinesContext = numberOfCodeLinesContext;
            _gateway = gateway;
        }
        public MergeChangeFrequenciesAndNumberOfCodeLinesResponse Execute(string repositoryUrl, string clocPath)
        {
            if (!String.IsNullOrWhiteSpace(repositoryUrl))
            {
                var cloneRepositoryResponse = _cloneRepositoryContext.Execute(repositoryUrl);

                if (cloneRepositoryResponse.IsSuccess)
                {   
                    var generateCsvWithChangeFrequenciesAndAuthorsResponse = _changeFrequenciesContext.Execute(cloneRepositoryResponse.ClonedRepositoryPath);
                    var generateCsvWithNumberOfCodeLinesResponse = _numberOfCodeLinesContext.Execute(cloneRepositoryResponse.ClonedRepositoryPath,clocPath);
                    var repositoryName = Path.GetFileNameWithoutExtension(cloneRepositoryResponse.ClonedRepositoryPath);
                    var csvFileName = $"{repositoryName}_complexity_metrics.csv";
                    var csvFilePath = Path.Combine(cloneRepositoryResponse.ClonedRepositoryPath, csvFileName);

                    if (generateCsvWithChangeFrequenciesAndAuthorsResponse.IsSuccess && generateCsvWithNumberOfCodeLinesResponse.IsSuccess)
                    {
                        var generateCsvResponse = GenerateMergedCsv(generateCsvWithChangeFrequenciesAndAuthorsResponse.GeneratedCsvPath, generateCsvWithNumberOfCodeLinesResponse.GeneratedCsvPath, csvFilePath);
                        if (generateCsvResponse)
                            return new MergeChangeFrequenciesAndNumberOfCodeLinesResponse { IsSuccess = true, MergedCsvFilePath = csvFilePath };
                        return new MergeChangeFrequenciesAndNumberOfCodeLinesResponse { IsSuccess = false, Error = "Error trying to generate the csv file!" };
                    }

                    if (!generateCsvWithChangeFrequenciesAndAuthorsResponse.IsSuccess) 
                        return new MergeChangeFrequenciesAndNumberOfCodeLinesResponse { IsSuccess = false, Error = generateCsvWithChangeFrequenciesAndAuthorsResponse.Error };
                    if (!generateCsvWithNumberOfCodeLinesResponse.IsSuccess) 
                        return new MergeChangeFrequenciesAndNumberOfCodeLinesResponse { IsSuccess = false, Error = generateCsvWithNumberOfCodeLinesResponse.Error };
                }
                return new MergeChangeFrequenciesAndNumberOfCodeLinesResponse { IsSuccess = false, Error = cloneRepositoryResponse.Error };
            }
            return new MergeChangeFrequenciesAndNumberOfCodeLinesResponse { IsSuccess = false, Error = "Repository url is empty!" };
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
                //parts[0] = module path ; parts[1] = changeFrequency
            }

            for (int i = 1; i < numberOfCodeLinesFile.Length; i++)
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
            }
            var sortedMetrics = SortDescendingAfterChangeFrequencyAndCodeSize(mergedMetrics);
            var response = _gateway.CreateCsvFileWithChangeFrequencyAndNumberOfCodeLines(sortedMetrics, csvFilePath);
            return response;
        }

        public Dictionary<string,ChangeFrequencyAndCodeMetric> SortDescendingAfterChangeFrequencyAndCodeSize(Dictionary<string,ChangeFrequencyAndCodeMetric> metrics)
        {
            return metrics.OrderByDescending(kv => kv.Value.Revisions)
                .ThenByDescending(kv => kv.Value.CodeLines)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }

    public class MergeChangeFrequenciesAndNumberOfCodeLinesResponse : BaseResponse
    {
        public string MergedCsvFilePath { get; set; }
    }
}
