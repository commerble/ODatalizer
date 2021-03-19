using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ODatalizer.EFCore;
using Sample.EFCore.Controllers;
using Sample.EFCore.Data;

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
            services.AddODatalizer();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SampleDbContext sample)
        {
            var ep = new ODatalizerEndpoint(
                            db:sample, 
                            routeName:"Sample", 
                            routePrefix:"sample", 
                            controller:nameof(SampleController), 
                            authorize: TestSettings.UseAuthorize,
                            @namespace: TestSettings.Namespace);

            SampleDbInitializer.Initialize(sample);

            app.UseODatalizer(ep);

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
                endpoints.MapODatalizer(ep);
                endpoints.MapControllers();
            });
        }
    }
}
