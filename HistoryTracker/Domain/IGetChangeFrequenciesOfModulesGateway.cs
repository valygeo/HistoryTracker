

namespace Domain
{
    public interface IGetChangeFrequenciesOfModulesGateway
    {
        public bool CreateCsvFileWithChangeFrequenciesOfModules(Dictionary<string, int> dictionary);
    }
}
