using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ODatalizer.EFCore;
using Sample.EFCore.Data;

namespace Sample.EFCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connection = new SqliteConnection("datasource=:memory:");
            connection.Open();
            services.AddDbContext<SampleDbContext>(opt => opt.UseSqlite(connection).UseLazyLoadingProxies());
            services.AddODatalizer();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SampleDbContext sample)
        {
            var ep = new ODatalizerEndpoint(sample, "Sample", "sample");

            SampleDbInitializer.Initialize(sample);

            app.UseODatalizer(ep);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapODatalizer(ep);
                endpoints.MapControllers();
            });

            System.GC.Collect();
        }
    }
}
