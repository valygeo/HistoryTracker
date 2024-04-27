
using Domain;
using Domain.Entities;

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
                    var authors = string.Join(";", module.Authors);
                    writer.WriteLine($"{module.EntityPath}, {module.Revisions}, {authors}");
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
