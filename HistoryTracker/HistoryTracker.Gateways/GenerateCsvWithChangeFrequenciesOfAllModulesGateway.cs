
using Domain;
using Domain.MetaData;

namespace HistoryTracker.Gateways
{
    public class GenerateCsvWithChangeFrequenciesOfAllModulesGateway : IGenerateCsvWithChangeFrequenciesOfAllModulesGateway
    {
        public bool CreateCsvFileWithChangeFrequenciesOfModules(
            Dictionary<string, int> modulesChangeFrequenciesAndAuthors, string csvFilePath)
        {
            using (var writer = new StreamWriter(csvFilePath))
            {
                writer.WriteLine("Module, Frequency");
                foreach (var module in modulesChangeFrequenciesAndAuthors)
                {
                    writer.WriteLine($"{module.Key},{module.Value}");
                }
            }
            return true;
        }
    }
}
