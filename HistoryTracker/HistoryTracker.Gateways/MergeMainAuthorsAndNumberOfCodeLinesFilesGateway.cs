
using Domain;
using Domain.MetaData;

namespace HistoryTracker.Gateways
{
    public class MergeMainAuthorsAndNumberOfCodeLinesFilesGateway : IMergeMainAuthorsAndNumberOfCodeLinesFilesGateway
    {
        public bool CreateCsvFileWithMainAuthorsAndNumberOfCodeLines(ICollection<FileMainAuthorsAndNumberOfCodeLines> metrics, string csvFilePath)
        {
            try
            {
                using (var writer = new StreamWriter(csvFilePath))
                {
                    writer.WriteLine("Module, Revisions, Code, Main Author");
                    foreach (var metric in metrics)
                    {
                        writer.WriteLine($"{metric.EntityPath},{metric.Revisions},{metric.CodeLines},{metric.MainAuthor}");
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
