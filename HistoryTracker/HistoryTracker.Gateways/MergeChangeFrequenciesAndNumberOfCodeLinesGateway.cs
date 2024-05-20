
using Domain;
using Domain.MetaData;

namespace HistoryTracker.Gateways
{
    public class MergeChangeFrequenciesAndNumberOfCodeLinesGateway : IMergeChangeFrequenciesAndNumberOfCodeLinesGateway
    {
        public bool CreateCsvFileWithChangeFrequencyAndNumberOfCodeLines(ICollection<ChangeFrequencyAndCodeMetric> metrics, string csvFilePath)
        {
            try
            {
                using (var writer = new StreamWriter(csvFilePath))
                {
                    writer.WriteLine("Module, Revisions, Code");
                    foreach (var metric in metrics)
                    {
                        writer.WriteLine($"{metric.EntityPath},{metric.Revisions},{metric.CodeLines}");
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
