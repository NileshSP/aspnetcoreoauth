using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aspnetcoreoauth.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using aspnetcoreoauth.Models;
using aspnetcoreoauth.Services;
using Microsoft.Extensions.Logging;
using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using FluentValidation.AspNetCore;

namespace aspnetcoreoauth
{
    public class Startup
    {
        private static readonly WindsorContainer _container = new WindsorContainer();
        private readonly ILoggerFactory _loggerFactory;
        private string _googleClientId = null;
        private string _googleClientSecret = null;
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
            _loggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var logger = _loggerFactory.CreateLogger<Startup>();

            if (HostingEnvironment.IsDevelopment())
            {
                logger.LogInformation("Development environment");
                _googleClientId = Configuration["Authentication:Google:ClientId"];
                _googleClientSecret = Configuration["Authentication:Google:ClientSecret"];
            }
            else
            {
                logger.LogInformation($"Environment: {HostingEnvironment.EnvironmentName}");
                _googleClientId = Environment.GetEnvironmentVariable("Authentication_Google_ClientId");
                _googleClientSecret = Environment.GetEnvironmentVariable("Authentication_Google_ClientSecret");
                logger.LogInformation($"Environment variables: Google - ClientId : {_googleClientId}, ClientSecret : {_googleClientSecret}");
            }
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDefaultIdentity<IdentityUser>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = _googleClientId;
                    options.ClientSecret = _googleClientSecret;
                });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase()
                //options.UseSqlServer(
                //    Configuration.GetConnectionString("DefaultConnection"))
                );

            //services.AddTransient<IApplicationDBContext>(s => s.GetRequiredService<ApplicationDbContext>());
            //services.AddTransient<IEThorEntityService, EThorEntityService>();

            services.AddMvcCore().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var assemblyName = typeof(aspnetcoreoauth.Startup).Assembly.GetName().Name;
            _container.Register(Component.For<IApplicationDBContext, ApplicationDbContext>().LifestyleTransient());
            _container.Register(Component.For<IEThorEntityService, EThorEntityService>().LifestyleTransient());
            _container.Register(
                Classes.FromAssemblyNamed(assemblyName).Pick().If(p => p.Name.EndsWith("Controller"))
                .LifestyleTransient());

            return WindsorRegistrationHelper.CreateServiceProvider(_container, services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<Startup> loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

    }
}
