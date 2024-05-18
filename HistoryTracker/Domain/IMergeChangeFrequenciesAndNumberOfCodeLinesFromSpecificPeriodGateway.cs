
using Domain.MetaData;

namespace Domain
{
    public interface IMergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodGateway
    {
        bool CreateCsvFileWithChangeFrequencyAndNumberOfCodeLines(ICollection<ChangeFrequencyAndCodeMetric> metrics, string csvFilePath);
    }
}
