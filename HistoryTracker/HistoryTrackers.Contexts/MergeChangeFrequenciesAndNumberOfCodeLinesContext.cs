
using System.Web;
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
                repositoryUrl = HttpUtility.UrlDecode(repositoryUrl);
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
                            return new MergeChangeFrequenciesAndNumberOfCodeLinesResponse
                                { IsSuccess = true, MergedCsvFilePath = csvFilePath };
                        return new MergeChangeFrequenciesAndNumberOfCodeLinesResponse
                            { IsSuccess = false, Error = "Error trying to generate the csv file!" };
                    }

                    if (!generateCsvWithChangeFrequenciesAndAuthorsResponse.IsSuccess) 
                        return new MergeChangeFrequenciesAndNumberOfCodeLinesResponse { IsSuccess = false, Error = generateCsvWithChangeFrequenciesAndAuthorsResponse.Error };
                    if (!generateCsvWithNumberOfCodeLinesResponse.IsSuccess) 
                        return new MergeChangeFrequenciesAndNumberOfCodeLinesResponse { IsSuccess = false, Error = generateCsvWithNumberOfCodeLinesResponse.Error };
                }
                return new MergeChangeFrequenciesAndNumberOfCodeLinesResponse
                    { IsSuccess = false, Error = cloneRepositoryResponse.Error };
            }
            return new MergeChangeFrequenciesAndNumberOfCodeLinesResponse
                { IsSuccess = false, Error = "Repository url is empty!" };

        }

        private bool GenerateMergedCsv(string changeFrequenciesCsvPath, string numberOfCodeLinesCsvPath, string csvFilePath)
        {
            var changeFrequenciesFile = File.ReadAllLines(changeFrequenciesCsvPath);
            var numberOfCodeLinesFile = File.ReadAllLines(numberOfCodeLinesCsvPath);
            var changeFrequenciesMetrics = new List<ChangeFrequency>();
            var numberOfCodeLinesMetrics = new List<CodeMetric>();
            var mergedProperties = new List<ChangeFrequencyAndCodeMetric>();

            for (int i = 1; i < changeFrequenciesFile.Length; i++)
            {
                var parts = changeFrequenciesFile[i].Split(',', 2);
                changeFrequenciesMetrics.Add(new ChangeFrequency
                {
                    EntityPath = parts[0],
                    Revisions = int.Parse(parts[1]),
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
                    mergedProperties.Add(new ChangeFrequencyAndCodeMetric
                    {
                        EntityPath = codeMetric.EntityPath,
                        CodeLines = codeMetric.CodeLines,
                        Revisions = matchingChangeFrequency.Revisions,
                    });
                }
            }
            var sortedMetrics = SortAfterChangeFrequencyAndCodeSize(mergedProperties);
            var response = _gateway.CreateCsvFileWithChangeFrequencyAndNumberOfCodeLines(sortedMetrics, csvFilePath);
            return response;
        }

        public List<ChangeFrequencyAndCodeMetric> SortAfterChangeFrequencyAndCodeSize(List<ChangeFrequencyAndCodeMetric> metrics)
        {
            var sortedMetrics = metrics.OrderByDescending(metric => metric.Revisions)
                .ThenByDescending(metric => metric.CodeLines).ToList();
            return sortedMetrics;
        }
        
    }

    public class MergeChangeFrequenciesAndNumberOfCodeLinesResponse : BaseResponse
    {
        public string MergedCsvFilePath { get; set; }
    }
}
