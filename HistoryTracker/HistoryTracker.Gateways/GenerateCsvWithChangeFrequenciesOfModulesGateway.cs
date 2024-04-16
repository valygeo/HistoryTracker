
using Domain;

namespace HistoryTracker.Gateways
{
    public class GenerateCsvWithChangeFrequenciesOfModulesGateway : IGenerateCsvWithChangeFrequenciesOfModulesGateway
    {
        public bool CreateCsvFileWithChangeFrequenciesOfModules(Dictionary<string, int> dictionary, string clonedRepositoryPath)
        {
            var repositoryName = Path.GetFileNameWithoutExtension(clonedRepositoryPath);
            var csvFileName = $"{repositoryName}ChangeFrequenciesOfModules.csv";
            var csvFilePath = Path.Combine(clonedRepositoryPath, csvFileName);
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
