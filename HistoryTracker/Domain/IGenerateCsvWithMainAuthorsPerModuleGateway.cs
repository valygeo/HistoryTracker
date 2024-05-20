
using Domain.MetaData;

namespace Domain
{
    public interface IGenerateCsvWithMainAuthorsPerModuleGateway
    {
        bool CreateCsvFileWithMainAuthorsAndChangeFrequenciesOfModules(
            ICollection<FileMainAuthor> modulesChangeFrequenciesAndMainAuthors, string csvFilePath);
    }
}
