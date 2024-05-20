
namespace Domain.MetaData
{
    public class Parent
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public double Weight { get; set; }
        public ICollection<Child>? Children { get; set; }
   
    }

    public class Child
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public double Weight { get; set; }

        public ICollection<Child>? Children { get; set; }

        public Parent ConvertToParent(Child child)
        {
            return new Parent
            {
                Name = child.Name,
                Value = child.Value,
                Children = child.Children,
                Weight = child.Weight
            };
        }
    }
}
