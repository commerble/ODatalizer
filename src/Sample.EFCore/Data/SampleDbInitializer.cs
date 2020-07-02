using Sample.EFCore.Entities;
using System;

namespace Sample.EFCore.Data
{
    public class SampleDbInitializer
    {
        public static void Initialize(SampleDbContext db)
        {
            db.SalesPatterns.Add(new SalesPattern { TaxRoundMode = TaxRoundMode.Round, TaxRate = 0.1m });
            db.Products.Add(new Product { Name = "Sample 1", UnitPrice = 1m, SalesPatternId = 1 });
            db.Products.Add(new Product { Name = "Sample 2", UnitPrice = 2m, SalesPatternId = 1 });
            db.Products.Add(new Product { Name = "Sample 3", UnitPrice = 3m, SalesPatternId = 1 });
            db.Products.Add(new Product { Name = "Sample 4", UnitPrice = 4m, SalesPatternId = 1 });
            db.Products.Add(new Product { Name = "Sample 5", UnitPrice = 5m, SalesPatternId = 1 });
            db.SalesProducts.Add(new SalesProduct { ProductId = 1, TaxRoundMode = TaxRoundMode.None });
            db.Campaigns.Add(new Campaign { Name = "Campaign 1", StartDate = DateTimeOffset.MinValue, EndDate = DateTimeOffset.MaxValue });
            db.Categories.Add(new Category { Name = "Category 1" });
            db.Categories.Add(new Category { Name = "Category 2" });
            db.Categories.Add(new Category { Name = "Category 3" });
            db.CampaignCategoryRelations.Add(new CampaignCategoryRelation { CampaignId = 1, CategoryId = 1 });
            db.CampaignCategoryRelations.Add(new CampaignCategoryRelation { CampaignId = 1, CategoryId = 2 });
            db.SaveChanges();
        }
    }
}
