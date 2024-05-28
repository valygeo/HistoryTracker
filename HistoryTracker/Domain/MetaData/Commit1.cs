
namespace Domain.MetaData
{
    public class Commit1
    {
        public string Author { get; set; }
        public string Message { get; set; }
        public Dictionary<string, CommitDetails1> CommitDetails { get; set; } = new Dictionary<string, CommitDetails1>();
        public string CommitDate { get; set; }

    }

    public class CommitDetails1
    {
        public int RowsAdded { get; set; }
        public int RowsDeleted { get; set; }
    }

}
