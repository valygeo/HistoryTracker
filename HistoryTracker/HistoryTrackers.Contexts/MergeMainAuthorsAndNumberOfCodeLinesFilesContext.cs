
using Domain;
using Domain.MetaData;
using HistoryTracker.Contexts.Base;
using System.Web;

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
                            return new MergeMainAuthorsAndNumberOfCodeLinesFilesResponse
                            { IsSuccess = true, MergedCsvFilePath = csvFilePath };
                        return new MergeMainAuthorsAndNumberOfCodeLinesFilesResponse
                        { IsSuccess = false, Error = "Error trying to generate the csv file!" };
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
            var changeFrequenciesMetrics = new List<FileMainAuthor>();
            var numberOfCodeLinesMetrics = new List<CodeMetric>();
            var mergedProperties = new List<FileMainAuthorsAndNumberOfCodeLines>();

            for (int i = 1; i < changeFrequenciesFile.Length; i++)
            {
                var parts = changeFrequenciesFile[i].Split(',', 3);
                changeFrequenciesMetrics.Add(new FileMainAuthor
                {
                    EntityPath = parts[0],
                    Revisions = int.Parse(parts[1]),
                    MainAuthor = parts[2]
                });
            }

            for (int i = 1; i < numberOfCodeLinesFile.Length - 1; i++)
            {
                var parts = numberOfCodeLinesFile[i].Split(",", 5);
                numberOfCodeLinesMetrics.Add(new CodeMetric
                {
                    ProgrammingLanguage = parts[0],
                    EntityPath = parts[1],
                    BlankLines = int.Parse(parts[2]),
                    CommentLines = int.Parse(parts[3]),
                    CodeLines = int.Parse(parts[4]),
                });
            }

            foreach (var codeMetric in numberOfCodeLinesMetrics)
            {
                var matchingChangeFrequency = changeFrequenciesMetrics.FirstOrDefault(entity => entity.EntityPath.Equals(codeMetric.EntityPath));
                if (matchingChangeFrequency != null)
                {
                    mergedProperties.Add(new FileMainAuthorsAndNumberOfCodeLines
                    {
                        EntityPath = codeMetric.EntityPath,
                        CodeLines = codeMetric.CodeLines,
                        Revisions = matchingChangeFrequency.Revisions,
                        MainAuthor = matchingChangeFrequency.MainAuthor
                    });
                }
                else
                {
                    mergedProperties.Add(new FileMainAuthorsAndNumberOfCodeLines
                    {
                        EntityPath = codeMetric.EntityPath,
                        CodeLines = codeMetric.CodeLines,
                        Revisions = 0,
                    });
                }
            }
            var sortedMetrics = SortAfterChangeFrequencyAndCodeSize(mergedProperties);
            var response = _gateway.CreateCsvFileWithMainAuthorsAndNumberOfCodeLines(sortedMetrics, csvFilePath);
            return response;
        }

        public List<FileMainAuthorsAndNumberOfCodeLines> SortAfterChangeFrequencyAndCodeSize(List<FileMainAuthorsAndNumberOfCodeLines> metrics)
        {
            var sortedMetrics = metrics.OrderByDescending(metric => metric.Revisions)
                .ThenByDescending(metric => metric.CodeLines).ToList();
            return sortedMetrics;
        }
    }

    public class MergeMainAuthorsAndNumberOfCodeLinesFilesResponse : BaseResponse
    {
        public string MergedCsvFilePath { get; set; }
    }
}
