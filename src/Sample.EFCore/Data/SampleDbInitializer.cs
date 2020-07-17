using Sample.EFCore.Entities;
using System;

namespace Sample.EFCore.Data
{
    public class SampleDbInitializer
    {
        public static void Initialize(SampleDbContext db)
        {
            db.Database.EnsureCreated();
            db.SalesPatterns.Add(new SalesPattern { TaxRoundMode = TaxRoundMode.Round, TaxRate = 0.1m });
            db.SaveChanges();
            db.Products.Add(new Product { Name = "Sample 1", UnitPrice = 1m, SalesPatternId = 1 });
            db.Products.Add(new Product { Name = "Sample 2", UnitPrice = 2m, SalesPatternId = 1 });
            db.Products.Add(new Product { Name = "Sample 3", UnitPrice = 3m, SalesPatternId = 1 });
            db.Products.Add(new Product { Name = "Sample 4", UnitPrice = 4m, SalesPatternId = 1 });
            db.Products.Add(new Product { Name = "Sample 5", UnitPrice = 5m, SalesPatternId = 1 });
            db.SaveChanges();

            db.SalesProducts.Add(new SalesProduct { ProductId = 1, TaxRoundMode = TaxRoundMode.None });
            db.SaveChanges();

            db.Campaigns.Add(new Campaign { Name = "Campaign 1", StartDate = DateTimeOffset.MinValue, EndDate = DateTimeOffset.MaxValue });
            db.SaveChanges();

            db.CampaignActions.Add(new CampaignAction { CampaignId = 1, CampaignType = "Sample", OptionValue = "Option 1" });
            db.SaveChanges();

            db.Categories.Add(new Category { Name = "Category 1" });
            db.Categories.Add(new Category { Name = "Category 2" });
            db.Categories.Add(new Category { Name = "Category 3" });
            db.SaveChanges();

            db.CampaignCategoryRelations.Add(new CampaignCategoryRelation { CampaignId = 1, CategoryId = 1 });
            db.CampaignCategoryRelations.Add(new CampaignCategoryRelation { CampaignId = 1, CategoryId = 2 });
            db.CampaignProductRelations.Add(new CampaignProductRelation { CampaignId = 1, ProductId = 1 });
            db.ProductCategoryRelations.Add(new ProductCategoryRelation { ProductId = 1, CategoryId = 1 });
            db.SaveChanges();
        }
    }
}
