using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData;
using ODatalizer.EFCore;
using Sample.EFCore.Controllers;
using Sample.EFCore.Data;
using System;
using System.Collections.Generic;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

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
            var sqlsvr = Configuration.GetConnectionString("SqlSvrConnection");

            if (string.IsNullOrEmpty(sqlsvr))
            {
                var connection = new SqliteConnection("datasource=:memory:");
                connection.Open();
                services.AddDbContext<SampleDbContext>(opt =>
                     opt.UseSqlite(connection)
                        .UseLazyLoadingProxies()
                        .ConfigureWarnings(o => o.Ignore(RelationalEventId.AmbientTransactionWarning)));
            }
            else
            {
                services.AddDbContext<SampleDbContext>(opt => opt.UseSqlServer(sqlsvr).UseLazyLoadingProxies());
            }
            services.AddODatalizer();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SampleDbContext sample)
        {
            var ep = new ODatalizerEndpoint(sample, "Sample", "sample", nameof(SampleController));

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
        }
    }
}
