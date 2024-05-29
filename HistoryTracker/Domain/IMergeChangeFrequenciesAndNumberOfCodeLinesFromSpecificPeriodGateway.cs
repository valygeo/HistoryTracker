
using Domain.MetaData;

namespace Domain
{
    public interface IMergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodGateway
    {
        bool CreateCsvFileWithChangeFrequencyAndNumberOfCodeLines(Dictionary<string, ChangeFrequencyAndCodeMetric> metrics, string csvFilePath);
    }
}
