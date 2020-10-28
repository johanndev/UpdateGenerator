namespace Entities
{
    public partial class Author : Entity<Author>
    {
        public string Readonly => "foo";
        public MyComplexClass MyComplexClass { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class MyComplexClass
    {
        public string MyProperty { get; set; }
    }
}
