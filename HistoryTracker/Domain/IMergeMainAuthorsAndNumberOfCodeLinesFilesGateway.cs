
using Domain.MetaData;

namespace Domain
{
    public interface IMergeMainAuthorsAndNumberOfCodeLinesFilesGateway
    {
        bool CreateCsvFileWithMainAuthorsAndNumberOfCodeLines(ICollection<FileMainAuthorsAndNumberOfCodeLines> metrics,
            string csvFilePath);
    }
}
