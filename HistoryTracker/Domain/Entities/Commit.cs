
namespace Domain.Entities
{
    public class Commit
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public string Message { get; set; }
        //public ICollection<EntityRows> EntityChanged { get; set; }

        public Commit(string id, string author, string message)
        {
            Id = id;
            Author = author;
            Message = message;
           
        }
    }

    public class EntityRows
    {
        public int RowsAdded { get; set; }
        public int RowsDeleted { get; set; }
        public string EntityName { get; set; }

        public EntityRows(int rowsAdded, int rowsDeleted, string entityName)
        {
            RowsAdded = rowsAdded;
            RowsDeleted = rowsDeleted;
            EntityName = entityName;
        }
    }

}
