
using Domain;


namespace HistoryTracker.Gateways
{
    public class GetSummaryDataGateway : IGetSummaryDataGateway
    {
        public string GetSummaryData(string logFilePath)
        {
                if (File.Exists(logFilePath))
                {
                    return File.ReadAllText(logFilePath);
                }

                {
                    Console.WriteLine("Error: Git log file not found.");
                    return null;
                }
            
        }
}
}
