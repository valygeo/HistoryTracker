
using Domain.MetaData;

namespace Domain
{
    public interface IMergeMainAuthorsAndNumberOfCodeLinesFilesGateway
    {
        bool CreateCsvFileWithMainAuthorsAndNumberOfCodeLines(Dictionary<string, FileMainAuthorsAndNumberOfCodeLines> metrics, string csvFilePath);
    }
}
