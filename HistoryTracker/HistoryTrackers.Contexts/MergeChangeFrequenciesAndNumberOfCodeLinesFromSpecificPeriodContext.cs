
using Domain;
using HistoryTracker.Contexts.Base;
using System.Web;
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
                request.RepositoryUrl = HttpUtility.UrlDecode(request.RepositoryUrl);
                var cloneRepositoryResponse = _cloneRepositoryContext.Execute(request.RepositoryUrl);

                if (cloneRepositoryResponse.IsSuccess)
                {
                    var repositoryName = Path.GetFileNameWithoutExtension(cloneRepositoryResponse.ClonedRepositoryPath);
                    var csvFileName = $"{repositoryName}_complexity_metrics_from_{request.StartDatePeriod:yyyy-MM-dd}_to_{request.EndDatePeriod:yyyy-MM-dd}.csv";
                    var csvFilePath = Path.Combine(cloneRepositoryResponse.ClonedRepositoryPath, csvFileName);
                    var generateCsvWithChangeFrequenciesAndAuthorsResponse =
                        _generateCsvWithChangeFrequenciesOfModules.Execute(new CreateLogFileFromSpecifiedPeriodData
                        {
                            clonedRepositoryPath = cloneRepositoryResponse.ClonedRepositoryPath,
                            repositoryName = repositoryName,
                            periodStartDate = request.StartDatePeriod,
                            periodEndDate = request.EndDatePeriod,
                        });

                    var generateCsvWithNumberOfCodeLinesResponse = _generateCsvWithNumberOfCodeLines.Execute(cloneRepositoryResponse.ClonedRepositoryPath, clocPath);
                    if (generateCsvWithChangeFrequenciesAndAuthorsResponse.IsSuccess && generateCsvWithNumberOfCodeLinesResponse.IsSuccess)
                    {
                        var generateCsvResponse = GenerateMergedCsv(generateCsvWithChangeFrequenciesAndAuthorsResponse.GeneratedCsvPath, generateCsvWithNumberOfCodeLinesResponse.GeneratedCsvPath, csvFilePath);
                        if (generateCsvResponse)
                            return new MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodResponse
                            { IsSuccess = true, MergedCsvFilePath = csvFilePath };
                        return new MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodResponse
                        { IsSuccess = false, Error = "Error trying to generate the csv file!" };
                    }

                    if (!generateCsvWithChangeFrequenciesAndAuthorsResponse.IsSuccess)
                        return new MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodResponse { IsSuccess = false, Error = generateCsvWithChangeFrequenciesAndAuthorsResponse.Error };
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
            var changeFrequenciesMetrics = new List<ChangeFrequency>();
            var numberOfCodeLinesMetrics = new List<CodeMetric>();
            var mergedProperties = new List<ChangeFrequencyAndCodeMetricPerFileForSpecificPeriod>();

            for (int i = 1; i < changeFrequenciesFile.Length; i++)
            {
                var parts = changeFrequenciesFile[i].Split(',', 3);
                changeFrequenciesMetrics.Add(new ChangeFrequency
                {
                    EntityPath = parts[0],
                    Revisions = int.Parse(parts[1]),
                    Authors = parts[2]
                });
            }

            for (int i = 1; i < numberOfCodeLinesFile.Length; i++)
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
                    mergedProperties.Add(new ChangeFrequencyAndCodeMetricPerFileForSpecificPeriod
                    {
                        EntityPath = codeMetric.EntityPath,
                        CodeLines = codeMetric.CodeLines,
                        Revisions = matchingChangeFrequency.Revisions,
                        Authors = matchingChangeFrequency.Authors,
                        WasModifiedInSpecificPeriod = true
                    });
                }
                else
                {
                    mergedProperties.Add(new ChangeFrequencyAndCodeMetricPerFileForSpecificPeriod
                    {
                        EntityPath = codeMetric.EntityPath,
                        CodeLines = codeMetric.CodeLines,
                        WasModifiedInSpecificPeriod = false,
                        Revisions = 0,
                        Authors = ""
                    });
                }
            }
            var sortedMetrics = SortAfterChangeFrequencyAndCodeSize(mergedProperties);
            var response = _gateway.CreateCsvFileWithChangeFrequencyAndNumberOfCodeLines(sortedMetrics, csvFilePath);
            return response;
        }

        public List<ChangeFrequencyAndCodeMetricPerFileForSpecificPeriod> SortAfterChangeFrequencyAndCodeSize(List<ChangeFrequencyAndCodeMetricPerFileForSpecificPeriod> metrics)
        {
            var sortedMetrics = metrics.OrderByDescending(metric => metric.Revisions)
                .ThenByDescending(metric => metric.CodeLines).ToList();
            return sortedMetrics;
        }

    }

    public class MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodResponse : BaseResponse
    {
        public string MergedCsvFilePath { get; set; }
    }
}
