
using Domain;
using Domain.Entities;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class MergeChangeFrequenciesAndNumberOfCodeLinesContext
    {
        private readonly IMergeChangeFrequenciesAndNumberOfCodeLinesGateway _gateway;

        public MergeChangeFrequenciesAndNumberOfCodeLinesContext(IMergeChangeFrequenciesAndNumberOfCodeLinesGateway gateway)
        {
            _gateway = gateway;
        }

        public MergeChangeFrequenciesAndNumberOfCodeLinesResponse Execute(string changeFrequenciesCsvPath, string numberOfCodeLinesCsvPath)
        {
            var changeFrequenciesFile = File.ReadAllLines(changeFrequenciesCsvPath);
            var numberOfCodeLinesFile = File.ReadAllLines(numberOfCodeLinesCsvPath);
            var changeFrequenciesMetrics = new List<ChangeFrequency>();
            var numberOfCodeLinesMetrics = new List<CodeMetric>();

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
            return new MergeChangeFrequenciesAndNumberOfCodeLinesResponse();
        }

    }

    public class MergeChangeFrequenciesAndNumberOfCodeLinesResponse : BaseResponse
    {

    }
}
