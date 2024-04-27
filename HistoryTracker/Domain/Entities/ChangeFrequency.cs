
namespace Domain.Entities
{
    public class ChangeFrequency
    {
        public string EntityPath { get; set; }
        public int Revisions { get; set; }
        public List<string> Authors { get; set; }
    }
}
