﻿
using Domain;
using Domain.Entities;

namespace HistoryTracker.Gateways
{
    public class MergeChangeFrequenciesAndNumberOfCodeLinesGateway : IMergeChangeFrequenciesAndNumberOfCodeLinesGateway
    {
        public bool CreateCsvFileWithChangeFrequencyAndNumberOfCodeLines(ICollection<ChangeFrequencyAndCodeMetric> metrics, string csvFilePath)
        {
            using (var writer = new StreamWriter(csvFilePath))
            {
                writer.WriteLine("Module, Revisions, Code, Authors");
                foreach (var metric in metrics)
                {
                    var authors = string.Join(";", metric.Authors);
                    writer.WriteLine($"{metric.EntityPath}, {metric.Revisions}, {metric.CodeLines}, {authors}");
                }
            }
            return true;
        }
    }
}
