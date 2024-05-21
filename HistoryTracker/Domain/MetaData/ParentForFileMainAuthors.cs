
namespace Domain.MetaData
{
    public class ParentForFileMainAuthors
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public double Weight { get; set; }
        public string MainAuthor { get; set; }
        public ICollection<ChildForFileMainAuthors>? Children { get; set; }
   
    }

    public class ChildForFileMainAuthors
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public double Weight { get; set; }
        public string MainAuthor { get; set; }
        public ICollection<ChildForFileMainAuthors>? Children { get; set; }

        public ParentForFileMainAuthors ConvertToParent(ChildForFileMainAuthors child)
        {
            return new ParentForFileMainAuthors
            {
                Name = child.Name,
                Value = child.Value,
                Children = child.Children,
                Weight = child.Weight,
                MainAuthor = child.MainAuthor,
            };
        }
    }
}
