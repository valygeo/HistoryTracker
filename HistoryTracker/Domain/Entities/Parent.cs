
namespace Domain.Entities
{
    public class Parent
    {
        public string ModuleName { get; set; }
        public int ModuleSize { get; set; }
        public int Revisions { get; set; }
        public string Authors { get; set; }
        public ICollection<Child>? Children { get; set; }
   
    }

    public class Child
    {
        public Parent Parent { get; set; }
        public string ModuleName { get; set; }
        public int ModuleSize { get; set; }
        public int Revisions { get; set; }

    public string Authors { get; set; }
        public ICollection<Child>? Children { get; set; }

        public Parent ConvertToParent(Child child)
        {
            return new Parent
            {
                ModuleName = child.ModuleName,
                ModuleSize = child.ModuleSize,
                Authors = child.Authors,
                Children = child.Children,
            };
        }
    }
}
