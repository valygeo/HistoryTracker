
using Domain.Entities;

namespace Domain
{
    public interface IGenerateCsvWithChangeFrequenciesOfModulesGateway
    {
        bool CreateCsvFileWithChangeFrequenciesOfModules(ICollection<ChangeFrequency> dictionary, string csvFilePath);
        bool CsvAlreadyExists(string csvFilePath);
    }
}
