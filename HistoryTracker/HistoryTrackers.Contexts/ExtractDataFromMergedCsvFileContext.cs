
using Domain.Entities;
using HistoryTracker.Contexts.Base;

namespace HistoryTracker.Contexts
{
    public class ExtractDataFromMergedCsvFileContext
    {
        public ExtractDataFromMergedCsvFileResponse Execute(string csvFilePath)
        {
            var metrics = new List<ChangeFrequencyAndCodeMetric>();
            var hierarchy = new List<Parent>();
            using (var reader = new StreamReader(csvFilePath))
            {
                reader.ReadLine();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(",");
                    var filePath = parts[0];
                    var metric = new ChangeFrequencyAndCodeMetric
                    {
                        EntityPath = filePath,
                        Revisions = int.Parse(parts[1]),
                        CodeLines = int.Parse(parts[2]),
                        Authors = parts[3]
                    };
                    metrics.Add(metric);
                    var pathParts = filePath.Split("\\");
                    AddToHierarchy(hierarchy, pathParts, metric);
                }
            }
            return new ExtractDataFromMergedCsvFileResponse { IsSuccess = true, ComplexityMetrics = metrics };
        }
        private void AddToHierarchy(List<Parent> hierarchy, string[] pathParts, ChangeFrequencyAndCodeMetric metric)
        {
            Parent parent = null;
            var currentLevel = hierarchy;

            for (int i = 1; i < pathParts.Length; i++)
            {
                var part = pathParts[i];

                parent = currentLevel.FirstOrDefault(p => p.ModuleName == part);

                if (parent == null)
                {
                    parent = new Parent
                    {
                        ModuleName = part,
                        Children = new List<Child>()
                    };
                    currentLevel.Add(parent);
                }
                else
                {
                    var child = new Child
                    {
                        ModuleName = pathParts[pathParts.Length - 1],
                        ModuleSize = metric.CodeLines,
                        Authors = metric.Authors,
                        Children = new List<Child>() 
                    };

                    parent.Children.Add(child);
                }
            }

        }
    }

    public class ExtractDataFromMergedCsvFileResponse : BaseResponse
    {
        public ICollection<ChangeFrequencyAndCodeMetric> ComplexityMetrics { get; set; }
    }
}
