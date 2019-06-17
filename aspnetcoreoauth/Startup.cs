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

            //services.AddTransient<IEThorEntityService, EThorEntityService>();

            //services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());

            services.AddMvcCore(options =>
                        {
                            options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                        })
                        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                        .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());

            //services.AddHttpContextAccessor();

            //services.AddLogging((lb) => lb.AddConsole().AddDebug());
            //services.AddSingleton<FrameworkMiddleware>(); // Do this if you don't care about using Windsor to inject dependencies

            // Custom application component registrations, ordering is important here
            RegisterApplicationComponents(services);

            //return WindsorRegistrationHelper.CreateServiceProvider(_container, services);
            return services.AddWindsor(_container,
                                        opts => opts.UseEntryAssembly(typeof(HomeController).Assembly), // <- Recommended
                                        () => services.BuildServiceProvider(validateScopes: false) // <- Optional
                                    );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<Startup> loggerFactory, IHttpContextAccessor httpContextAccessor)
        {
            // For making component registrations of middleware easier
            _container.GetFacility<AspNetCoreFacility>().RegistersMiddlewareInto(app);

            // Add custom middleware, do this if your middleware uses DI from Windsor
            _container.Register(Component.For<CustomLoggingMiddleware>()
                                            .DependsOn(Dependency.OnValue<ILoggerFactory>(loggerFactory))
                                            //.DependsOn(Dependency.OnValue<IHttpContextAccessor>(httpContextAccessor))
                                            .LifestyleTransient().AsMiddleware());

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
            _container.Register(Component.For<IHttpContextAccessor>().ImplementedBy<HttpContextAccessor>());
            _container.Register(Component.For<IApplicationDBContext>().ImplementedBy<ApplicationDbContext>().LifestyleTransient().CrossWired());
            _container.Register(Component.For<IEThorEntityService, EThorEntityService>().LifestyleTransient());
            _container.Register(Classes.FromAssemblyNamed(assemblyName).Pick().If(p => p.Name.EndsWith("Controller")).LifestyleTransient());
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

    // Example of some custom user-defined middleware component. Resolves types from Windsor.
    public sealed class CustomLoggingMiddleware : IMiddleware
    {
        private readonly ILogger<CustomLoggingMiddleware> _logger;

        public CustomLoggingMiddleware(ILogger<CustomLoggingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await next(context);

            var request = context.Request;
            var response = context.Response;
            _logger.LogInformation($"Custom Middleware logging: \nRequest: {request.Scheme} {request.Host}{request.Path} {request.QueryString} \nResponse code: {response.StatusCode} ");
        }
    }
}