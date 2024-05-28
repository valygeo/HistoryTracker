
using Domain.MetaData;

namespace Domain
{
    public interface IGenerateCsvWithChangeFrequenciesOfAllModulesGateway
    {
        bool CreateCsvFileWithChangeFrequenciesOfModules(Dictionary<string, int> changeFrequenciesOfModules, string csvFilePath);
    }
}
