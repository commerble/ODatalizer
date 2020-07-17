using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.EFCore.Data;
using System.Linq;

namespace ODatalizer.EFCore.Tests.Host
{
    public class ODatalizerWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var connection = new SqliteConnection("datasource=:memory:");
                connection.Open();
                // Create a new service provider.
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkProxies()
                    .AddEntityFrameworkSqlite()
                    .BuildServiceProvider();

                // Add a database context (AppDbContext) using an in-memory database for testing.
                services.Remove(services.First(descriptor => descriptor.ServiceType == typeof(SampleDbContext)));
                services.Remove(services.First(descriptor => descriptor.ServiceType == typeof(DbContextOptions)));
                services.Remove(services.First(descriptor => descriptor.ServiceType == typeof(DbContextOptions<SampleDbContext>)));
                services.AddDbContext<SampleDbContext>(options =>
                {
                    options
                        .UseSqlite(connection)
                        .UseLazyLoadingProxies()
                        .ConfigureWarnings(o => o.Ignore(RelationalEventId.AmbientTransactionWarning))
                        .UseInternalServiceProvider(serviceProvider);
                });
            });
        }
    }
}
