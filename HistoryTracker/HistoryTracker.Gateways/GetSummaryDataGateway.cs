﻿
using Domain;


namespace HistoryTracker.Gateways
{
    public class GetSummaryDataGateway : IGetSummaryDataGateway
    {
        public ICollection<string> GetSummaryData(string logFilePath)
        {
            if (File.Exists(logFilePath))
            { 
                var fields = new List<string>();
                var lines = File.ReadAllLines(logFilePath);
                foreach (var line in lines)
                {
                    fields.Add(line);
                }
                return fields;
            }
            return new List<string>();
        }
}
}
