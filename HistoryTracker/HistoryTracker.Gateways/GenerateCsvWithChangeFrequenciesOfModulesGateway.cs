
using Domain;
using Domain.MetaData;

namespace HistoryTracker.Gateways
{
    public class GenerateCsvWithChangeFrequenciesOfModulesGateway : IGenerateCsvWithChangeFrequenciesOfModulesGateway
    {
        public bool CreateCsvFileWithChangeFrequenciesOfModules(ICollection<ChangeFrequency> modulesChangeFrequenciesAndAuthors, string csvFilePath)
        {
            using (var writer = new StreamWriter(csvFilePath))
            {
                writer.WriteLine("Module, Frequency, Authors");
                foreach (var module in modulesChangeFrequenciesAndAuthors)
                {
                    writer.WriteLine($"{module.EntityPath},{module.Revisions},{module.Authors}");
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
