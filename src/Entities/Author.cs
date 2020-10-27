using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public partial class Author : Entity<Author>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
