
using Domain;

namespace HistoryTracker.Gateways
{
    public class GenerateCsvWithChangeFrequenciesOfModulesGateway : IGenerateCsvWithChangeFrequenciesOfModulesGateway
    {
        public bool CreateCsvFileWithChangeFrequenciesOfModules(Dictionary<string, int> dictionary, string csvFilePath)
        {
            using (var writer = new StreamWriter(csvFilePath))
            {
                writer.WriteLine("Module, Frequency");

                foreach (var kvp in dictionary)
                {
                    writer.WriteLine($"{kvp.Key}, {kvp.Value}");
                }
            }
            return true;
        }

        public bool CsvAlreadyExists(string csvFilePath)
        {
            if (File.Exists(csvFilePath))
                return true;
            return false;
        }
    }
}
