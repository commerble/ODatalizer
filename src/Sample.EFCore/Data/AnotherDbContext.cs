using Microsoft.EntityFrameworkCore;
using Sample.EFCore.AnotherEntities;
using System.Diagnostics.CodeAnalysis;

namespace Sample.EFCore.Data
{
    public class AnotherDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public AnotherDbContext([NotNull] DbContextOptions<AnotherDbContext> options) : base(options)
        {
        }
    }
}
