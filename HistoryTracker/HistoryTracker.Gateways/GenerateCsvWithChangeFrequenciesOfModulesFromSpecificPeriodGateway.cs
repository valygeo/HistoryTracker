
using Domain;

namespace HistoryTracker.Gateways
{
    public class GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodGateway : IGenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodGateway
    {
        public bool CreateCsvFileWithChangeFrequenciesOfModules(Dictionary<string,int> modulesChangeFrequencies, string csvFilePath)
        {
            using (var writer = new StreamWriter(csvFilePath))
            {
                writer.WriteLine("Module, Frequency");
                foreach (var module in modulesChangeFrequencies)
                {
                    writer.WriteLine($"{module.Key},{module.Value}");
                }
            }
            return true;
        }
    }
}
