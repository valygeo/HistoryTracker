
using Domain.MetaData;

namespace Domain
{
    public interface IMergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodGateway
    {
        bool CreateCsvFileWithChangeFrequencyAndNumberOfCodeLines(ICollection<ChangeFrequencyAndCodeMetricPerFileForSpecificPeriod> metrics, string csvFilePath);
    }
}
