using Microsoft.EntityFrameworkCore;
using Sample.EFCore.AnotherEntities;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Sample.EFCore.Data
{
    public class AnotherDbContext : DbContext
    {
        public DbSet<A> As { get; set; }
        public DbSet<B> Bs { get; set; }
        public DbSet<C> Cs { get; set; }
        public DbSet<D> Ds { get; set; }
        public DbSet<E> Es { get; set; }
        public AnotherDbContext([NotNull] DbContextOptions<AnotherDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<D>().HasKey(o => new { o.Id, o.EId });
        }
    }

    public class A
    {
        [Key]
        public int Id { get; set; }
        public int BId { get; set; }
        [Required]
        public string CId { get; set; }
        public string DId { get; set; }
        public string DEId { get; set; }
        public virtual B B { get; set; }
        public virtual C C { get; set; }
        public virtual D D { get; set; }
    }
    public class B
    {
        [Key]
        public int Id { get; set; }
    }
    public class C
    {
        [Key]
        public string Id { get; set; }
    }
    public class D
    {
        [Key]
        public string Id { get; set; }
        [Key]
        public string EId { get; set; }
    }
    public class E
    {
        [Key]
        public string Id { get; set; }
    }
}
