
namespace Domain.MetaData
{
    public class CodeMetric
    {
        public string ProgrammingLanguage { get; set; }
        public string EntityPath { get; set; }
        public int BlankLines { get; set; }
        public int CommentLines { get; set; }
        public int CodeLines { get; set; }
    }
}
