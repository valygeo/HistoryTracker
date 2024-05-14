
using Domain.MetaData;

namespace Domain
{
    public interface IGenerateCsvWithChangeFrequenciesOfModulesGateway
    {
        bool CreateCsvFileWithChangeFrequenciesOfModules(ICollection<ChangeFrequency> changeFrequenciesOfModules, string csvFilePath);
        bool CsvAlreadyExists(string csvFilePath);
    }
}
