
using Domain;

namespace HistoryTracker.Gateways
{
    public class ExtractAllCommitsGateway : IExtractAllCommitsGateway
    {
        public ICollection<string> ReadLogFile(string logFilePath)
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
