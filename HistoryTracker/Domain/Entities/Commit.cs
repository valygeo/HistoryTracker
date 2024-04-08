
namespace Domain.Entities
{
    public class Commit
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public string Message { get; set; }
        public ICollection<CommitDetails> CommitDetails { get; set; }
        public string CommitDate { get; set; }

    }

    public class CommitDetails
    {
        public int RowsAdded { get; set; }
        public int RowsDeleted { get; set; }
        public string EntityChangedName { get; set; }
    }

}
