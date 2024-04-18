
using Domain;
using Domain.Entities;

namespace HistoryTracker.Gateways
{
    public class MergeChangeFrequenciesAndNumberOfCodeLinesGateway : IMergeChangeFrequenciesAndNumberOfCodeLinesGateway
    {
        public bool CreateCsvFileWithChangeFrequencyAndNumberOfCodeLines(ICollection<ChangeFrequency> changeFrequenciesMetric, ICollection<CodeMetric> codeMetric)
        {

            return true;
        }
    }
}
