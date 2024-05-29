
using Domain.MetaData;

namespace Domain
{
    public interface IGenerateCsvWithMainAuthorsPerModuleGateway
    {
        bool CreateCsvFileWithMainAuthorsAndChangeFrequenciesOfModules(Dictionary<string, FileMainAuthor> modulesChangeFrequenciesAndMainAuthors, string csvFilePath);
    }
}
