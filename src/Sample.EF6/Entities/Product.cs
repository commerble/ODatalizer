using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.EF6.Entities
{
    public class Product
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public int SalesPatternId { get; set; }
        public virtual SalesPattern SalesPattern { get; set; }
        public virtual SalesProduct SalesProduct { get; set; }

        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
        public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
    }
}
