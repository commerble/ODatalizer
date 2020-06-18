using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.EFCore;
using Sample.EFCore.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ODatalizer.EFCore.Tests.Host
{
    public class ODatalizerWebApplicationFactory<TStartup> : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Create a new service provider.
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                // Add a database context using an in-memory database for testing.
                services.AddDbContext<SampleDbContext>(options =>
                {
                    options.UseInMemoryDatabase("e2etest");
                    options.UseInternalServiceProvider(serviceProvider);
                });

                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database contexts
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var sample = scopedServices.GetRequiredService<SampleDbContext>();

                    var logger = scopedServices.GetRequiredService<ILogger<ODatalizerWebApplicationFactory<TStartup>>>();

                    // Ensure the database is created.
                    sample.Database.EnsureCreated();
                }
            });
        }
    }
}
