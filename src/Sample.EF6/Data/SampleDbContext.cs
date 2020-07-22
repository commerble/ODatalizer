using Sample.EF6.Entities;
using System.Data.Common;
using System.Data.Entity;

namespace Sample.EF6.Data
{
    [DbConfigurationType(typeof(SampleDbConfiguration))]
    public class SampleDbContext : DbContext
    {
        public SampleDbContext(DbConnection existingConnection) : base(existingConnection, contextOwnsConnection: true)
        {
        }

        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignAction> CampaignActions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<SalesPattern> SalesPatterns { get; set; }
        public DbSet<SalesProduct> SalesProducts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>()
                            .HasKey(o => o.Id);

            modelBuilder.Entity<Campaign>()
                            .HasKey(o => o.Id);

            modelBuilder.Entity<CampaignAction>()
                            .HasKey(o => new { o.CampaignId, o.CampaignType });

            modelBuilder.Entity<Product>()
                            .HasKey(o => o.Id);

            modelBuilder.Entity<SalesPattern>()
                            .HasKey(o => o.Id);

            modelBuilder.Entity<SalesProduct>()
                            .HasKey(o => o.ProductId);

            modelBuilder.Entity<SalesPattern>()
                            .HasMany(o => o.Products)
                            .WithRequired(o => o.SalesPattern)
                            .HasForeignKey(o => o.SalesPatternId);

            modelBuilder.Entity<Product>()
                            .HasOptional(o => o.SalesProduct)
                            .WithRequired(o => o.Product);

            modelBuilder.Entity<Campaign>()
                            .HasMany(o => o.Actions)
                            .WithRequired(o => o.Campaign)
                            .HasForeignKey(o => o.CampaignId);

            modelBuilder.Entity<Product>()
                            .HasMany(o => o.Categories)
                            .WithMany(o => o.Products)
                            .Map(c => c.ToTable("ProductCategoryRelations").MapLeftKey("ProductId").MapRightKey("CategoryId"));

            modelBuilder.Entity<Campaign>()
                            .HasMany(o => o.Categories)
                            .WithMany(o => o.Campaigns)
                            .Map(c => c.ToTable("CampaignCategoryRelations").MapLeftKey("CampaignId").MapRightKey("CategoryId"));

            modelBuilder.Entity<Campaign>()
                            .HasMany(o => o.Products)
                            .WithMany(o => o.Campaigns)
                            .Map(c => c.ToTable("CampaignProductRelations").MapLeftKey("CampaignId").MapRightKey("ProductId"));

            base.OnModelCreating(modelBuilder);
        }
    }
}
