
using Domain.MetaData;

namespace Domain
{
    public interface IGenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodGateway
    {
        bool CreateCsvFileWithChangeFrequenciesOfModules(ICollection<ChangeFrequency> changeFrequenciesOfModules, string csvFilePath);
    }
}
