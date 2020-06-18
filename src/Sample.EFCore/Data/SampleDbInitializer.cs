using Sample.EFCore.Entities;

namespace Sample.EFCore.Data
{
    public class SampleDbInitializer
    {
        public static void Initialize(SampleDbContext db)
        {
            db.SalesPatterns.Add(new SalesPattern { TaxRoundMode = TaxRoundMode.Round });
            db.SaveChanges();
        }
    }
}
