﻿
using Domain;
using Domain.MetaData;

namespace HistoryTracker.Gateways
{
    public class GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodGateway : IGenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodGateway
    {
        public bool CreateCsvFileWithChangeFrequenciesOfModules(ICollection<ChangeFrequency> modulesChangeFrequenciesAndAuthors, string csvFilePath)
        {
            using (var writer = new StreamWriter(csvFilePath))
            {
                writer.WriteLine("Module, Frequency");
                foreach (var module in modulesChangeFrequenciesAndAuthors)
                {
                    writer.WriteLine($"{module.EntityPath},{module.Revisions}");
                }
            }
            return true;
        }
    }
}
