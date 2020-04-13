using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using tracktor.app.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using tracktor.app.Helpers;
using tracktor.service;
using tracktor.model.DAL;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace tracktor.app
{
    public class Startup
    {
        private IWebHostEnvironment _env;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddAuthentication().AddCookie(o =>
            {
                o.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = context =>
                    {
                        return SecurityStampValidator.ValidatePrincipalAsync(context);
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                        return Task.FromResult(0);
                    },
                    OnRedirectToLogin = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                        return Task.FromResult(0);
                    }
                };
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(o =>
            {
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(1);
                o.Lockout.MaxFailedAccessAttempts = 3;
                o.Lockout.AllowedForNewUsers = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                options.Filters.AddService(typeof(AngularAntiforgeryCookieResultFilter));
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            }).AddNewtonsoftJson();

            services.Configure<SecurityStampValidatorOptions>(o =>
            {
                o.ValidationInterval = TimeSpan.Zero;
            });

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-Token");

            services.AddTransient<IEmailSender, EmailSender>(sp => new EmailSender(
                Configuration["Tracktor:SmtpHost"],
                Int32.Parse(Configuration["Tracktor:SmtpPort"]),
                Boolean.Parse(Configuration["Tracktor:SmtpSsl"]),
                Configuration["Tracktor:SmtpSender"],
                Configuration["Tracktor:SmtpUsername"],
                Configuration["Tracktor:SmtpPassword"]));

            services.AddScoped<ITracktorContext>(sp => new TracktorContext(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<ITracktorService, TracktorService>();
            services.AddSingleton(log4net.LogManager.GetLogger(GetType()));
            services.AddSingleton<IMapper>(TracktorStartup.AppInitialize());
            services.AddSingleton(Configuration);
            services.AddTransient<AngularAntiforgeryCookieResultFilter>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            _env = env;

            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseMiddleware(typeof(ExceptionHandling));
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            loggerFactory.AddLog4Net();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });

            Initialize(app.ApplicationServices).Wait();
        }

        protected async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

                // drop test db
                if (_env.EnvironmentName == "Test")
                {
                    await dbContext.Database.EnsureDeletedAsync();
                }

                if (dbContext.Database.GetPendingMigrations().Any())
                {
                    await dbContext.Database.MigrateAsync();

                    var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                    foreach (var role in new string[] { "User" })
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            await roleManager.CreateAsync(new IdentityRole(role));
                        }
                    }
                }
            }
        }
    }
}
