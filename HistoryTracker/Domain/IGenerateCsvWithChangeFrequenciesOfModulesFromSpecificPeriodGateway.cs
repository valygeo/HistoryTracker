
using Domain.MetaData;

namespace Domain
{
    public interface IGenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodGateway
    {
        bool CreateCsvFileWithChangeFrequenciesOfModules(Dictionary<string, int> modulesChangeFrequencies,
            string csvFilePath);
    }
}
