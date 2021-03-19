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
        }

        public IConfiguration Configuration { get; }

        public static bool UseAuthorize = true;

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

            if (UseAuthorize)
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
                            authorize: UseAuthorize);

            SampleDbInitializer.Initialize(sample);

            app.UseODatalizer(ep);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePages();

            app.UseHttpsRedirection();

            app.UseRouting();

            if (UseAuthorize)
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
