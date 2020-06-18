using Sample.EFCore.Entities;

namespace Sample.EFCore.Data
{
    public class SampleDbInitializer
    {
        public static void Initialize(SampleDbContext db)
        {
            db.SalesPatterns.Add(new SalesPattern { TaxRoundMode = TaxRoundMode.Round });
            db.Products.Add(new Product { Name = "Sample 1", UnitPrice = 1m, SalesPatternId = 1 });
            db.Products.Add(new Product { Name = "Sample 2", UnitPrice = 2m, SalesPatternId = 1 });
            db.Products.Add(new Product { Name = "Sample 3", UnitPrice = 3m, SalesPatternId = 1 });
            db.Products.Add(new Product { Name = "Sample 4", UnitPrice = 4m, SalesPatternId = 1 });
            db.Products.Add(new Product { Name = "Sample 5", UnitPrice = 5m, SalesPatternId = 1 });
            db.SaveChanges();
        }
    }
}
