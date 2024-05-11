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
                }
            }

            foreach (var metric in metrics)
            {
                var pathParts = metric.EntityPath.Split("\\");
                var existingParent = hierarchy.FirstOrDefault(p => p.Name == pathParts[1]);
       
                if (existingParent == null)
                {
                    var parent = new Parent
                    {
                        Name = pathParts[1],
                        Value = 0,
                        Revisions = 0,
                        Authors = "",
                        Children = new List<Child>()
                    };
                    hierarchy.Add(parent);
                    AddChildrensToHierarchy(parent, pathParts, metric, 2);
                }
                else
                {
                    AddChildrensToHierarchy(existingParent, pathParts, metric, 2);
                }
            }
            return new ExtractDataFromMergedCsvFileResponse { IsSuccess = true, ComplexityMetrics = metrics, Hierarchy = hierarchy};
        }

        private void AddChildrensToHierarchy(Parent parent, string[] pathParts, ChangeFrequencyAndCodeMetric metric, int index)
        {
            Child currentChild;
            if (index == pathParts.Length - 1)
            {
                var lastPart = pathParts[index];
                currentChild = parent.Children.FirstOrDefault(c => c.Name == lastPart);

                if (currentChild == null)
                {
                    currentChild = new Child
                    {
                        Name = lastPart,
                        Value = metric.CodeLines,
                        Authors = metric.Authors,
                        Revisions = metric.Revisions,
                        Children = new List<Child>()
                    };
                    parent.Children.Add(currentChild);
                }
                return; 
            }

            if (index >= pathParts.Length)
                return;

            var part = pathParts[index];
            currentChild = parent.Children.FirstOrDefault(c => c.Name == part);
            if (currentChild == null)
            {
                currentChild = new Child
                {
                    Name = part,
                    Value = 0,
                    Authors = "",
                    Children = new List<Child>()
                };
                parent.Children.Add(currentChild);
            }
            parent = currentChild.ConvertToParent(currentChild);

            AddChildrensToHierarchy(parent, pathParts, metric, index + 1);
        }


        public class ExtractDataFromMergedCsvFileResponse : BaseResponse
        {
            public ICollection<ChangeFrequencyAndCodeMetric> ComplexityMetrics { get; set; }
            public ICollection<Parent> Hierarchy { get; set; }
        }
    }
}
