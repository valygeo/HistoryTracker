
using Domain;

namespace HistoryTracker.Gateways
{
    public class GetChangeFrequenciesOfModulesGateway : IGetChangeFrequenciesOfModulesGateway
    {
        public bool CreateCsvFileWithChangeFrequenciesOfModules(Dictionary<string, int> dictionary)
        {
            var csvFileName = "changeFrequencies.csv";
            var csvFilePath = Path.Combine("C:\\Users\\Vali\\Documents\\ClonedRepositories\\HistoryTracker", csvFileName);
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
    }
}
