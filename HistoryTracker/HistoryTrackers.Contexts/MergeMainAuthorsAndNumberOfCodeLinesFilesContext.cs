
using Domain;
using Domain.MetaData;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class MergeMainAuthorsAndNumberOfCodeLinesFilesContext
    {
        private readonly CloneRepositoryContext _cloneRepositoryContext;
        private readonly GenerateCsvWithMainAuthorsPerModuleContext _generateCsvWithMainAuthorsPerModule;
        private readonly GenerateCsvWithNumberOfCodeLinesContext _generateCsvWithNumberOfCodeLines;
        private readonly IMergeMainAuthorsAndNumberOfCodeLinesFilesGateway _gateway;

        public MergeMainAuthorsAndNumberOfCodeLinesFilesContext(CloneRepositoryContext cloneRepositoryContext, GenerateCsvWithMainAuthorsPerModuleContext generateCsvWithMainAuthorsPerModule,
            GenerateCsvWithNumberOfCodeLinesContext generateCsvWithNumberOfCodeLinesContext, IMergeMainAuthorsAndNumberOfCodeLinesFilesGateway gateway)
        {
            _cloneRepositoryContext = cloneRepositoryContext;
            _generateCsvWithMainAuthorsPerModule = generateCsvWithMainAuthorsPerModule;
            _generateCsvWithNumberOfCodeLines = generateCsvWithNumberOfCodeLinesContext;
            _gateway = gateway;
        }

        public MergeMainAuthorsAndNumberOfCodeLinesFilesResponse Execute(ComplexityMetricsRequest request, string clocPath)
        {
            if (!String.IsNullOrWhiteSpace(request.RepositoryUrl))
            {
                var cloneRepositoryResponse = _cloneRepositoryContext.Execute(request.RepositoryUrl);
                if (cloneRepositoryResponse.IsSuccess)
                {
                    var repositoryName = Path.GetFileNameWithoutExtension(cloneRepositoryResponse.ClonedRepositoryPath);
                    
                    var generateCsvWithChangeFrequenciesAndMainAuthorsAuthorsResponse =
                        _generateCsvWithMainAuthorsPerModule.Execute(new CreateCsvFromSpecifiedPeriodRequest
                        {
                            ClonedRepositoryPath = cloneRepositoryResponse.ClonedRepositoryPath, 
                            RepositoryName = repositoryName,
                            PeriodEndDate = request.EndDatePeriod,
                        });

                    var generateCsvWithNumberOfCodeLinesResponse = _generateCsvWithNumberOfCodeLines.Execute(cloneRepositoryResponse.ClonedRepositoryPath, clocPath);
                    if (generateCsvWithChangeFrequenciesAndMainAuthorsAuthorsResponse.IsSuccess && generateCsvWithNumberOfCodeLinesResponse.IsSuccess)
                    {
                        var csvFileName = $"{repositoryName}_complexity_metrics_before_{request.EndDatePeriod:yyyy-MM-dd}.csv";
                        var csvFilePath = Path.Combine(cloneRepositoryResponse.ClonedRepositoryPath, csvFileName);
                        var generateCsvResponse = GenerateMergedCsv(generateCsvWithChangeFrequenciesAndMainAuthorsAuthorsResponse.GeneratedCsvPath, generateCsvWithNumberOfCodeLinesResponse.GeneratedCsvPath, csvFilePath);

                        if (generateCsvResponse)
                            return new MergeMainAuthorsAndNumberOfCodeLinesFilesResponse { IsSuccess = true, MergedCsvFilePath = csvFilePath };
                        return new MergeMainAuthorsAndNumberOfCodeLinesFilesResponse { IsSuccess = false, Error = "Error trying to generate the csv file!" };
                    }

                    if (!generateCsvWithChangeFrequenciesAndMainAuthorsAuthorsResponse.IsSuccess)
                        return new MergeMainAuthorsAndNumberOfCodeLinesFilesResponse { IsSuccess = false, Error = generateCsvWithChangeFrequenciesAndMainAuthorsAuthorsResponse.Error };
                    if (!generateCsvWithNumberOfCodeLinesResponse.IsSuccess)
                        return new MergeMainAuthorsAndNumberOfCodeLinesFilesResponse { IsSuccess = false, Error = generateCsvWithNumberOfCodeLinesResponse.Error };
                }
                return new MergeMainAuthorsAndNumberOfCodeLinesFilesResponse
                { IsSuccess = false, Error = cloneRepositoryResponse.Error };
            }
            return new MergeMainAuthorsAndNumberOfCodeLinesFilesResponse
            { IsSuccess = false, Error = "Repository url is empty!" };
        }
        private bool GenerateMergedCsv(string changeFrequenciesCsvPath, string numberOfCodeLinesCsvPath, string csvFilePath)
        {
            var changeFrequenciesFile = File.ReadAllLines(changeFrequenciesCsvPath);
            var numberOfCodeLinesFile = File.ReadAllLines(numberOfCodeLinesCsvPath);
            var changeFrequenciesAndMainAuthorsMetrics = new Dictionary<string, FileMainAuthor>();
            var numberOfCodeLinesMetrics = new Dictionary<string, int>();
            var mergedMetrics = new Dictionary<string, FileMainAuthorsAndNumberOfCodeLines>();

            for (int i = 1; i < changeFrequenciesFile.Length; i++)
            {
                var parts = changeFrequenciesFile[i].Split(',', 3);
                changeFrequenciesAndMainAuthorsMetrics.Add(parts[0], new FileMainAuthor
                {
                    Revisions = int.Parse(parts[1]),
                    MainAuthor = parts[2]
                });
            }

            for (int i = 1; i < numberOfCodeLinesFile.Length - 1; i++)
            {
                var parts = numberOfCodeLinesFile[i].Split(",", 5);
                numberOfCodeLinesMetrics.Add(parts[1], int.Parse(parts[4]));
                //parts[1] = module path; parts[4] = changeFrequency
            }

            foreach (var codeMetric in numberOfCodeLinesMetrics)
            {
                var existingModule = changeFrequenciesAndMainAuthorsMetrics.ContainsKey(codeMetric.Key);
                if (existingModule)
                {
                    mergedMetrics.Add(codeMetric.Key, new FileMainAuthorsAndNumberOfCodeLines
                    {
                        CodeLines = codeMetric.Value,
                        Revisions = changeFrequenciesAndMainAuthorsMetrics[codeMetric.Key].Revisions,
                        MainAuthor = changeFrequenciesAndMainAuthorsMetrics[codeMetric.Key].MainAuthor
                    });
                }
                else
                {
                    mergedMetrics.Add(codeMetric.Key, new FileMainAuthorsAndNumberOfCodeLines
                    {
                        CodeLines = codeMetric.Value,
                        Revisions = 0,
                        MainAuthor = ""
                    });
                }
            }
            var sortedMetrics = SortAfterChangeFrequencyAndCodeSize(mergedMetrics);
            var response = _gateway.CreateCsvFileWithMainAuthorsAndNumberOfCodeLines(sortedMetrics, csvFilePath);
            return response;
        }

        private static Dictionary<string, FileMainAuthorsAndNumberOfCodeLines> SortAfterChangeFrequencyAndCodeSize(Dictionary<string,FileMainAuthorsAndNumberOfCodeLines> metrics)
        {
            return metrics.OrderByDescending(kv => kv.Value.Revisions).ThenByDescending(kv => kv.Value.CodeLines)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }

    public class MergeMainAuthorsAndNumberOfCodeLinesFilesResponse : BaseResponse
    {
        public string MergedCsvFilePath { get; set; }
    }
}
