﻿using Microsoft.EntityFrameworkCore;
using Sample.EFCore.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Sample.EFCore.Data
{
    public class SampleDbContext : DbContext
    {
        public SampleDbContext([NotNull] DbContextOptions options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SalesPattern> SalesPatterns { get; set; }
        public DbSet<SalesProduct>SalesProducts { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignAction> CampaignActions { get; set; }
        public DbSet<ProductCategoryRelation> ProductCategoryRelations { get; set; }
        public DbSet<CampaignProductRelation> CampaignProductRelations { get; set; }
        public DbSet<CampaignCategoryRelation> CampaignCategoryRelations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
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

            modelBuilder.Entity<ProductCategoryRelation>()
                            .HasKey(o => new { o.ProductId, o.CategoryId });

            modelBuilder.Entity<CampaignProductRelation>()
                            .HasKey(o => new { o.CampaignId, o.ProductId });

            modelBuilder.Entity<CampaignCategoryRelation>()
                            .HasKey(o => new { o.CampaignId, o.CategoryId });

            modelBuilder.Entity<Product>()
                            .HasOne(o => o.SalesPattern)
                            .WithMany(o => o.Products)
                            .HasForeignKey(o => o.SalesPatternId);

            modelBuilder.Entity<Product>()
                            .HasOne(o => o.SalesProduct)
                            .WithOne(o => o.Product)
                            .HasForeignKey<SalesProduct>(o => o.ProductId);

            modelBuilder.Entity<Campaign>()
                            .HasMany(o => o.Actions)
                            .WithOne(o => o.Campaign)
                            .HasForeignKey(o => o.CampaignId);

            modelBuilder.Entity<ProductCategoryRelation>()
                            .HasOne(o => o.Product)
                            .WithMany(o => o.CategoryRelations)
                            .HasForeignKey(o => o.ProductId);

            modelBuilder.Entity<ProductCategoryRelation>()
                            .HasOne(o => o.Category)
                            .WithMany(o => o.ProductRelations)
                            .HasForeignKey(o => o.CategoryId);

            modelBuilder.Entity<CampaignProductRelation>()
                            .HasOne(o => o.Product)
                            .WithMany(o => o.CampaignRelations)
                            .HasForeignKey(o => o.ProductId);

            modelBuilder.Entity<CampaignProductRelation>()
                            .HasOne(o => o.Campaign)
                            .WithMany(o => o.ProductRelations)
                            .HasForeignKey(o => o.CampaignId);

            modelBuilder.Entity<CampaignCategoryRelation>()
                            .HasOne(o => o.Category)
                            .WithMany(o => o.CampaignRelations)
                            .HasForeignKey(o => o.CategoryId);

            modelBuilder.Entity<CampaignCategoryRelation>()
                            .HasOne(o => o.Campaign)
                            .WithMany(o => o.CategoryRelations)
                            .HasForeignKey(o => o.CampaignId);

            modelBuilder.Entity<Product>()
                            .Ignore(o => o.Categories);

            modelBuilder.Entity<Product>()
                            .Ignore(o => o.Campaigns);

            modelBuilder.Entity<Category>()
                            .Ignore(o => o.Products);

            modelBuilder.Entity<Category>()
                            .Ignore(o => o.Campaigns);

            modelBuilder.Entity<Campaign>()
                            .Ignore(o => o.Products);

            modelBuilder.Entity<Campaign>()
                            .Ignore(o => o.Categories);

            base.OnModelCreating(modelBuilder);
        }
    }
}
