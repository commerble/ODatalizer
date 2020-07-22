using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.EF6.Entities
{
    public class SalesProduct
    {
        public long ProductId { get; set; }
        public TaxRoundMode TaxRoundMode { get; set; }
        public decimal TaxRate { get; set; }

        public virtual Product Product { get; set; }
    }
}
