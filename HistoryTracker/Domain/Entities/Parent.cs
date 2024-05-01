
namespace Domain.Entities
{
    public class Parent
    {
        public string ModuleName { get; set; }
        public int ModuleSize { get; set; }
        public string Authors { get; set; }
        public ICollection<Child>? Children { get; set; }
   
    }

    public class Child
    {
        public string ModuleName { get; set; }
        public int ModuleSize { get; set; }
        public string Authors { get; set; }
        public ICollection<Child>? Children { get; set; }
    }
}
