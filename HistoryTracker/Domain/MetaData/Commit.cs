
namespace Domain.MetaData
{
    public class Commit
    {
        public string Author { get; set; }
        public string Message { get; set; }
        public Dictionary<string, CommitDetails> CommitDetails { get; set; } = new Dictionary<string, CommitDetails>();
        public string CommitDate { get; set; }

    }

    public class CommitDetails
    {
        public int RowsAdded { get; set; }
        public int RowsDeleted { get; set; }
    }

}
