
using Domain.MetaData;

namespace Domain
{
    public interface IMergeChangeFrequenciesAndNumberOfCodeLinesGateway
    {
        bool CreateCsvFileWithChangeFrequencyAndNumberOfCodeLines(ICollection<ChangeFrequencyAndCodeMetric> metrics, string csvFilePath);
    }
}
