using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var b1 = new Book
            {
                Title = "Book1"
            };
            var b2 = new Book
            {
                Title = "Book2"
            };

            Console.WriteLine($"Book 1 title: {b1.Title}");

            b1.Update(b2);

            Console.WriteLine($"Book 1 title after update: {b1.Title}");

            Console.WriteLine(Type.GetType("Entities.Book").AssemblyQualifiedName);

        }
    }
}
