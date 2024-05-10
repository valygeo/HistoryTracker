
namespace Domain.Entities
{
    public class Parent
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public int Revisions { get; set; }
        public string Authors { get; set; }
        public ICollection<Child>? Children { get; set; }
   
    }

    public class Child
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public int Revisions { get; set; }

        public string Authors { get; set; }
        public ICollection<Child>? Children { get; set; }

        public Parent ConvertToParent(Child child)
        {
            return new Parent
            {
                Name = child.Name,
                Size = child.Size,
                Authors = child.Authors,
                Children = child.Children,
            };
        }
    }
}
