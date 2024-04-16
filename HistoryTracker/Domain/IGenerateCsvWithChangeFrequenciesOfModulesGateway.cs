

namespace Domain
{
    public interface IGenerateCsvWithChangeFrequenciesOfModulesGateway
    {
        bool CreateCsvFileWithChangeFrequenciesOfModules(Dictionary<string, int> dictionary, string clonedRepositoryPath);
    }
}
