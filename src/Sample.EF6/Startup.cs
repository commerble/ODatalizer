using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ODatalizer.EF6;
using Sample.EF6.Data;

namespace Sample.EF6
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
            var sqlsvr = Configuration.GetConnectionString("SqlSvrConnection");
            var connection = new System.Data.SqlClient.SqlConnection(sqlsvr);

            services.AddSingleton(sp => new SampleDbContext(connection));
            services.AddODatalizer();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SampleDbContext sample)
        {
            var ep = new ODatalizerEndpoint(sample, "Sample", "sample");

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
        }
    }
}
