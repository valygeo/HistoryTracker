using Domain.Entities;
using HistoryTracker.Contexts.Base;


namespace HistoryTracker.Contexts
{
    public class ExtractDataFromMergedCsvFileContext
    {
        public ExtractDataFromMergedCsvFileResponse Execute(string csvFilePath)
        {
            var metrics = new List<ChangeFrequencyAndCodeMetric>();
            var parent = new Parent();
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
                }
            }

            foreach (var metric in metrics)
            {
                var pathParts = metric.EntityPath.Split("\\");
                var existingParent = hierarchy.FirstOrDefault(p => p.ModuleName == pathParts[1]);
       
                if (existingParent == null)
                {
                    parent = new Parent
                    {
                        ModuleName = pathParts[1],
                        ModuleSize = 0, 
                        Revisions = 0,
                        Authors = "",
                        Children = new List<Child>()
                    };
                    hierarchy.Add(parent);
                    AddToHierarchy(parent, pathParts, metric, 2);
                }
                else
                {
                    AddToHierarchy(existingParent, pathParts, metric, 2);
                }
            }
            return new ExtractDataFromMergedCsvFileResponse { IsSuccess = true, ComplexityMetrics = metrics, Hierarchy = hierarchy};
        }

        private void AddToHierarchy(Parent parent, string[] pathParts, ChangeFrequencyAndCodeMetric metric, int index)
        {
            if (index >= pathParts.Length)
                return;

            var part = pathParts[index];
            var currentChild = parent.Children.FirstOrDefault(c => c.ModuleName == part);

            if (currentChild == null)
            {
                currentChild = new Child
                {
                    ModuleName = part,
                    ModuleSize = 0,
                    Authors = "",
                    Parent = parent,
                    Children = new List<Child>()
                };
                parent.Children.Add(currentChild);
            }

            parent = currentChild.ConvertToParent(currentChild);
          
            AddToHierarchy(parent, pathParts, metric, index + 1);
        }

        public class ExtractDataFromMergedCsvFileResponse : BaseResponse
        {
            public ICollection<ChangeFrequencyAndCodeMetric> ComplexityMetrics { get; set; }
            public ICollection<Parent> Hierarchy { get; set; }
        }
    }
}
