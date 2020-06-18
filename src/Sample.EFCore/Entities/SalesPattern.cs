using System.Collections.Generic;

namespace Sample.EFCore.Entities
{
    public class SalesPattern
    {
        public int Id { get; set; }

        public TaxRoundMode TaxRoundMode { get; set; }
        public decimal TaxRate { get; set; }
        public virtual ICollection<Product> Products { get; set; }

    }
}
