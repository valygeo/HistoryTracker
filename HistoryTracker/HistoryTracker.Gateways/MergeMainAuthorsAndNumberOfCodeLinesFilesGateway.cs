
using Domain;
using Domain.MetaData;

namespace HistoryTracker.Gateways
{
    public class MergeMainAuthorsAndNumberOfCodeLinesFilesGateway : IMergeMainAuthorsAndNumberOfCodeLinesFilesGateway
    {
        public bool CreateCsvFileWithMainAuthorsAndNumberOfCodeLines(Dictionary<string, FileMainAuthorsAndNumberOfCodeLines> metrics, string csvFilePath)
        {
            try
            {
                using (var writer = new StreamWriter(csvFilePath))
                {
                    writer.WriteLine("Module, Revisions, Code, Main Author");
                    foreach (var metric in metrics)
                    {
                        writer.WriteLine($"{metric.Key},{metric.Value.Revisions},{metric.Value.CodeLines},{metric.Value.MainAuthor}");
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
