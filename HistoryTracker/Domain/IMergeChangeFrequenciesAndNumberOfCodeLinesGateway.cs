
using Domain.MetaData;

namespace Domain
{
    public interface IMergeChangeFrequenciesAndNumberOfCodeLinesGateway
    {
        bool CreateCsvFileWithChangeFrequencyAndNumberOfCodeLines(Dictionary<string,ChangeFrequencyAndCodeMetric> metrics, string csvFilePath);
    }
}
