
using Domain;
using Domain.MetaData;

namespace HistoryTracker.Gateways
{
    public class GenerateCsvWithMainAuthorsPerModuleGateway : IGenerateCsvWithMainAuthorsPerModuleGateway
    {
        public bool CreateCsvFileWithMainAuthorsAndChangeFrequenciesOfModules(ICollection<FileMainAuthor> modulesChangeFrequenciesAndMainAuthors, string csvFilePath)
        {
            using (var writer = new StreamWriter(csvFilePath))
            {
                writer.WriteLine("Module, Frequency, Main Author");
                foreach (var module in modulesChangeFrequenciesAndMainAuthors)
                {
                    writer.WriteLine($"{module.EntityPath},{module.Revisions},{module.MainAuthor}");
                }
            }
            return true;
        }
    }
}
