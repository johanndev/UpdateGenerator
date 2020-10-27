using System;

namespace Entities
{
    public partial class Book : Entity<Book>
    {
        public string Title { get; set; }
    }
}
