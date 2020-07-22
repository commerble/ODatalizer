using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.EF6.Entities
{
    public class SalesPattern
    {
        public int Id { get; set; }
        public TaxRoundMode TaxRoundMode { get; set; }
        public decimal TaxRate { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
