using Domain.MetaData;
using HistoryTracker.Contexts.Base;


namespace HistoryTracker.Contexts
{
    public class GetHotspotsFrequenciesAndComplexityPerFileFromAllTimeContext
    {
        public GetHotspotsFrequenciesAndComplexityPerFileFromAllTimeResponse Execute(string csvFilePath)
        {
            var hierarchy = new List<Parent>();
            var fileLines = File.ReadAllLines(csvFilePath);
            var maxRevisionsMetric = 0;

            for (int i = 1; i < fileLines.Length; i++)
            {
                var line = fileLines[i];
                var metricParts = line.Split(",");
                if (maxRevisionsMetric == 0)
                {
                    maxRevisionsMetric = int.Parse(metricParts[1]);
                }
                var modulePathParts = metricParts[0].Split("\\");
                var existingParent = hierarchy.FirstOrDefault(p => p.Name == modulePathParts[1]);

                if (existingParent == null)
                {
                    var parent = new Parent
                    {
                        Name = modulePathParts[1],
                        Value = 0,
                        Weight = 0,
                        Authors = "",
                        Children = new List<Child>()
                    };
                    hierarchy.Add(parent);
                    AddChildrenToHierarchy(parent, modulePathParts, metricParts, 2, maxRevisionsMetric);
                }
                else
                {
                    AddChildrenToHierarchy(existingParent, modulePathParts, metricParts, 2, maxRevisionsMetric);
                }
            }
            return new GetHotspotsFrequenciesAndComplexityPerFileFromAllTimeResponse { IsSuccess = true, Hierarchy = hierarchy};
        }

        private static void AddChildrenToHierarchy(Parent parent, string[] pathParts, string[] metricParts, int index, int maxRevisionsMetric)
        {
            Child currentChild;
            if (index == pathParts.Length - 1)
            {
                var lastPart = pathParts[index];
                currentChild = parent.Children.FirstOrDefault(c => c.Name == lastPart);
                var revisionsMetric = int.Parse(metricParts[1]);
                if (currentChild == null)
                {
                    currentChild = new Child
                    {
                        Name = lastPart,
                        Value = int.Parse(metricParts[2]),
                        Authors = metricParts[3],
                        Weight = (double)revisionsMetric/ maxRevisionsMetric,
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

            AddChildrenToHierarchy(parent, pathParts, metricParts, index + 1, maxRevisionsMetric);
        }


        public class GetHotspotsFrequenciesAndComplexityPerFileFromAllTimeResponse : BaseResponse
        {
            public ICollection<Parent> Hierarchy { get; set; }
        }
    }
}
