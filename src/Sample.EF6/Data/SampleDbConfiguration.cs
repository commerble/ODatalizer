using System.Collections.Concurrent;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;

namespace Sample.EF6.Data
{
    public class SampleDbConfiguration : DbConfiguration
    {
        public static ConcurrentDictionary<string, DbProviderFactory> ProviderFactories { get; } = new ConcurrentDictionary<string, DbProviderFactory>();
        public static ConcurrentDictionary<string, DbProviderServices> ProviderServices { get; } = new ConcurrentDictionary<string, DbProviderServices>();
        public SampleDbConfiguration()
        {
            foreach (var entry in ProviderFactories)
            {
                SetProviderFactory(entry.Key, entry.Value);
            }
            foreach (var entry in ProviderServices)
            {
                SetProviderServices(entry.Key, entry.Value);
            }

        }
    }
}
