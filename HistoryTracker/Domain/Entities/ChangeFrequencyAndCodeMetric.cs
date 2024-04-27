
namespace Domain.Entities
{
    public class ChangeFrequencyAndCodeMetric
    {
        public string EntityPath { get; set; }
        public int CodeLines { get; set; }
        public int Revisions { get; set; }
        public List<string> Authors { get; set; }
    }
}
