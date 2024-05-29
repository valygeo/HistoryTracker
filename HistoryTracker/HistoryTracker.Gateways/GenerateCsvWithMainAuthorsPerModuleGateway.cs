
using Domain;
using Domain.MetaData;

namespace HistoryTracker.Gateways
{
    public class GenerateCsvWithMainAuthorsPerModuleGateway : IGenerateCsvWithMainAuthorsPerModuleGateway
    {
        public bool CreateCsvFileWithMainAuthorsAndChangeFrequenciesOfModules(Dictionary<string, FileMainAuthor> modulesChangeFrequenciesAndMainAuthors, string csvFilePath)
        {
            using (var writer = new StreamWriter(csvFilePath))
            {
                writer.WriteLine("Module, Frequency, Main Author");
                foreach (var module in modulesChangeFrequenciesAndMainAuthors)
                {
                    writer.WriteLine($"{module.Key},{module.Value.Revisions},{module.Value.MainAuthor}");
                }
            }
            return true;
        }
    }
}
