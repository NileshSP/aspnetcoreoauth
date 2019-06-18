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
using Castle.Facilities.AspNetCore;
using aspnetcoreoauth.Controllers;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http.Internal;
using System.Text;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;

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
        public void ConfigureServices(IServiceCollection services)
        {
            // Setup component model contributors for making windsor services available to IServiceProvider
            _container.AddFacility<AspNetCoreFacility>(f => f.CrossWiresInto(services));

            var logger = _loggerFactory.CreateLogger<Startup>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Get OAuth Google creds/secrets
            var googleClientId = Configuration["OAuth:Google:ClientId"];
            var googleClientSecret = Configuration["OAuth:Google:ClientSecret"];

            logger.LogInformation($"Environment: {HostingEnvironment.EnvironmentName}");
            if (HostingEnvironment.IsDevelopment())
            {
                _googleClientId = Configuration[googleClientId];
                _googleClientSecret = Configuration[googleClientSecret];
            }
            else
            {
                _googleClientId = Environment.GetEnvironmentVariable(googleClientId);
                _googleClientSecret = Environment.GetEnvironmentVariable(googleClientSecret);
            }

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = _googleClientId;
                    options.ClientSecret = _googleClientSecret;
                });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase()
                //options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                );

            services.AddDefaultIdentity<IdentityUser>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc()
                        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                        .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());

            //services.AddSingleton<FrameworkMiddleware>(); // Do this if you don't care about using Windsor to inject dependencies

            // Custom application component registrations, ordering is important here
            RegisterApplicationComponents(services);

            //return WindsorRegistrationHelper.CreateServiceProvider(_container, services);
            services.AddWindsor(_container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<Startup> loggerFactory, IHttpContextAccessor httpContextAccessor)
        {
            // For making component registrations of middleware easier
            _container.GetFacility<AspNetCoreFacility>().RegistersMiddlewareInto(app);

            // Controller middleware resolve dependant service type 1 
            //var entityService = _container.Resolve<IEThorEntityService>();
            //if (entityService != null)
            //{
            //    _container.Register(Component.For<ResolveControllerDependenciesMiddleware>()
            //                                    .DependsOn(Dependency.OnValue<ILoggerFactory>(loggerFactory))
            //                                    .DependsOn(Dependency.OnValue<IEThorEntityService>(entityService))
            //                                    .LifestyleTransient()
            //                                    .AsMiddleware());
            //}

            // Add framework configured middleware, use this if you dont have any DI requirements
            //app.UseMiddleware<FrameworkMiddleware>();

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

            // Controller middleware resolve dependant service type 2 
            //app.Use(async(ctx,next) => 
            //{
            //    loggerFactory.LogInformation($"middleware to provide/resolve required dependent service for controller");
            //    var eThorService = _container.Resolve<IEThorEntityService>(); //ctx.RequestServices.GetService<IEThorEntityService>();
            //    await next();
            //});

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void RegisterApplicationComponents(IServiceCollection services)
        {
            var assemblyName = typeof(aspnetcoreoauth.Startup).Assembly.GetName().Name;
            _container.Register(Component.For<IApplicationDBContext>().ImplementedBy<ApplicationDbContext>().LifestyleTransient().CrossWired());
            _container.Register(Component.For<IEThorEntityService, EThorEntityService>().LifestyleTransient()); // Automatically resolves dependencies for controller
            //_container.Register(Classes.FromAssemblyNamed(assemblyName).Pick().If(p => p.Name.EndsWith("Controller")).LifestyleTransient()); // Not required
        }

    }

    // Example of framework configured middleware component, can't consume types registered in Windsor
    public class FrameworkMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Do something before
            await next(context);
            // Do something after
        }
    }

    public sealed class ResolveControllerDependenciesMiddleware : IMiddleware
    {
        private readonly ILogger<ResolveControllerDependenciesMiddleware> _logger;
        private readonly IEThorEntityService _service;

        public ResolveControllerDependenciesMiddleware(ILogger<ResolveControllerDependenciesMiddleware> logger, IEThorEntityService service)
        {
            _logger = logger;
            _service = service;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _logger.LogInformation($"Resolve service middleware logging: started.... ");

            await next(context);
            //var request = context.Request;
            //var response = context.Response;
            //_logger.LogInformation($"Custom Middleware logging: \nRequest: {request.Scheme} {request.Host}{request.Path} {request.QueryString} \nResponse code: {response.StatusCode} ");
            //_logger.LogInformation($"Custom Middleware logging: completed.... ");

            _logger.LogInformation($"Resolve service middleware logging: completed.... ");
        }
    }
}