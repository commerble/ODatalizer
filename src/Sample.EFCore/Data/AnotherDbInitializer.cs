namespace Sample.EFCore.Data
{
    public class AnotherDbInitializer
    {
        public static void Initialize(AnotherDbContext db)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}
