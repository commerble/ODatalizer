using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ODatalizer.EFCore;
using ODatalizer.EFCore.Converters;
using Sample.EFCore.Controllers;
using Sample.EFCore.Converters;
using Sample.EFCore.Data;
using System;

namespace Sample.EFCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            TestSettings = new TestSettings
            {
                UseAuthorize = Configuration.GetValue<bool?>("UseAuthorize") ?? true,
                Namespace = Configuration.GetValue<string>("Namespace"),
            };
        }

        public IConfiguration Configuration { get; }

        public TestSettings TestSettings { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var sqlsvr = Configuration.GetConnectionString("SqlSvrConnection");

            if (string.IsNullOrEmpty(sqlsvr))
            {
                var connection1 = new SqliteConnection("datasource=:memory:");
                connection1.Open();
                services.AddDbContext<SampleDbContext>(opt =>
                     opt.UseSqlite(connection1)
                        .UseLazyLoadingProxies()
                        .ConfigureWarnings(o => o.Ignore(RelationalEventId.AmbientTransactionWarning)));

                var connection2 = new SqliteConnection("datasource=:memory:");
                connection2.Open();
                services.AddDbContext<AnotherDbContext>(opt =>
                     opt.UseSqlite(connection2)
                        .UseLazyLoadingProxies()
                        .ConfigureWarnings(o => o.Ignore(RelationalEventId.AmbientTransactionWarning)));
            }
            else
            {
                services.AddDbContext<SampleDbContext>(opt => opt.UseSqlServer(sqlsvr).UseLazyLoadingProxies());
                services.AddDbContext<AnotherDbContext>(opt => opt.UseSqlServer(sqlsvr).UseLazyLoadingProxies());
            }

            services.Configure<TestSettings>(Configuration);
            if (TestSettings.UseAuthorize)
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Read", policy =>
                        policy.Requirements.Add(new OperationAuthorizationRequirement { Name = "Read" }));

                    options.AddPolicy("Write", policy =>
                        policy.Requirements.Add(new OperationAuthorizationRequirement { Name = "Write" }));
                });
                services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                        .AddCookie(opt => {
                            opt.Events.OnRedirectToAccessDenied = context =>
                            {
                                context.Response.StatusCode = 403;
                                return System.Threading.Tasks.Task.CompletedTask;
                            };
                            opt.Events.OnRedirectToLogin = context =>
                            {
                                context.Response.StatusCode = 401;
                                return System.Threading.Tasks.Task.CompletedTask;
                            };
                        });
            }

            services.AddSingleton<IAuthorizationHandler, SampleAuthorizationHandler>();
          
            services.AddODatalizer(sp =>
            {
                var ep = new ODatalizerEndpoint(
                            db: sp.GetRequiredService<SampleDbContext>(),
                            routeName: "Sample",
                            routePrefix: "sample",
                            controller: nameof(SampleController),
                            authorize: TestSettings.UseAuthorize,
                            @namespace: TestSettings.Namespace);

                var an = new ODatalizerEndpoint(
                            db: sp.GetRequiredService<AnotherDbContext>(),
                            routeName: "Another",
                            routePrefix: "another");

                return new[] { ep, an }; 
            });
          
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ITypeConverter, DateTimeConverter>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SampleDbContext sample, AnotherDbContext another)
        {
            SampleDbInitializer.Initialize(sample);
            AnotherDbInitializer.Initialize(another);

            app.UseODatalizer();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePages();

            app.UseHttpsRedirection();

            app.UseRouting();

            if (TestSettings.UseAuthorize)
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
