namespace Sample.EFCore.Entities
{
    public class SalesProduct
    {
        public long ProductId { get; set; }

        public TaxRoundMode TaxRoundMode { get; set; }
        public decimal TaxRate { get; set; }

        public virtual Product Product { get; set; }
    }
}
