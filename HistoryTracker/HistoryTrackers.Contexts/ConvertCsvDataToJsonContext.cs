
using Domain.Entities;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class ConvertCsvDataToJsonContext
    {
        public ConvertCsvDataToJsonResponse Execute(string csvFilePath)
        {
            var metrics = new List<ChangeFrequencyAndCodeMetric>();
            using (var reader = new StreamReader(csvFilePath))
            {
                reader.ReadLine();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(",");
                    var metric = new ChangeFrequencyAndCodeMetric
                    {
                        EntityPath = parts[0],
                        Revisions = int.Parse(parts[1]),
                        CodeLines = int.Parse(parts[2]),
                        Authors = parts[3]
                    };
                    metrics.Add(metric);
                }
            }
            return new ConvertCsvDataToJsonResponse { IsSuccess = true , ComplexityMetrics = metrics};
        }
    }

    public class ConvertCsvDataToJsonResponse : BaseResponse
    {
        public ICollection<ChangeFrequencyAndCodeMetric> ComplexityMetrics { get; set; }
    }
}
