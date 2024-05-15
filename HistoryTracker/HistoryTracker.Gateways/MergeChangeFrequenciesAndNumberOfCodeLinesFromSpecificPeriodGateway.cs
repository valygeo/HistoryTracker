
using Domain;
using Domain.MetaData;

namespace HistoryTracker.Gateways
{
    public class MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodGateway : IMergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodGateway
    {
        public bool CreateCsvFileWithChangeFrequencyAndNumberOfCodeLines(ICollection<ChangeFrequencyAndCodeMetricPerFileForSpecificPeriod> metrics, string csvFilePath)
        {
            try
            {
                using (var writer = new StreamWriter(csvFilePath))
                {
                    writer.WriteLine("Module, Revisions, Code, Authors");
                    foreach (var metric in metrics)
                    {
                        var authors = string.Join(";", metric.Authors);
                        writer.WriteLine($"{metric.EntityPath},{metric.Revisions},{metric.CodeLines},{authors}");
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
