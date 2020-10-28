using System;
using System.Collections.Generic;

namespace Entities
{
    public partial class Book : Entity<Book>
    {
        public Author Author { get; set; }
        public string Title { get; set; }
        public ISBN ISBN { get; set; }
        public BookType BookType { get; set; }
        public int Int { get; set; }
        public decimal Decimal { get; set; }
        public DateTime DateTime { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public Guid Guid { get; set; }

        public int? NullableInt { get; set; }
        public decimal? NullableDecimal { get; set; }
        public DateTime? NullableDateTime { get; set; }
        public DateTimeOffset? NullableDateTimeOffset { get; set; }
        public TimeSpan? NullableTimeSpan { get; set; }
        public Guid? NullableGuid { get; set; }

        public IEnumerable<int> IEnumerable { get; set; }
    }
}
