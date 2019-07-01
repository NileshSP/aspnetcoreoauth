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
using System.Diagnostics;
using HtmlAgilityPack;

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
            //var entityService = _container.Resolve<ISampleEntityService>();
            //if (entityService != null)
            //{
            //    _container.Register(Component.For<ResolveControllerDependenciesMiddleware>()
            //                                    .DependsOn(Dependency.OnValue<ILoggerFactory>(loggerFactory))
            //                                    .DependsOn(Dependency.OnValue<ISampleEntityService>(entityService))
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

            app.UseMiddleware<ResponseMeasurementMiddleware>();

            // Controller middleware resolve dependant service type 2 
            //app.Use(async(ctx,next) => 
            //{
            //    loggerFactory.LogInformation($"middleware to provide/resolve required dependent service for controller");
            //    var SampleService = _container.Resolve<ISampleEntityService>(); //ctx.RequestServices.GetService<ISampleEntityService>();
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
            _container.Register(Component.For<ISampleEntityService, SampleEntityService>().LifestyleTransient()); // Automatically resolves dependencies for controller
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
        private readonly ISampleEntityService _service;

        public ResolveControllerDependenciesMiddleware(ILogger<ResolveControllerDependenciesMiddleware> logger, ISampleEntityService service)
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

    public class ResponseMeasurementMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseMeasurementMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var originalBody = context.Response.Body;
            var newBody = new MemoryStream();
            context.Response.Body = newBody;

            var watch = new Stopwatch();
            long responseTime = 0;
            watch.Start();
            await _next(context);
            //// read the new body
            // read the new body
            responseTime = watch.ElapsedMilliseconds;
            newBody.Position = 0;
            var newContent = await new StreamReader(newBody).ReadToEndAsync();
            // calculate the updated html
            var updatedHtml = CreateDataNode(newContent, responseTime);
            // set the body = updated html
            var updatedStream = GenerateStreamFromString(updatedHtml);
            await updatedStream.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

        }
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        private string CreateDataNode(string originalHtml, long responseTime)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(originalHtml);
            HtmlNode testNode = HtmlNode.CreateNode($"<div class='text-muted text-center small' style='width:100vw; position:absolute;z-index:5' >Response Time: {responseTime.ToString()} ms.</div>");
            var htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//body");
            htmlBody.InsertBefore(testNode, htmlBody.FirstChild);

            string rawHtml = htmlDoc.DocumentNode.OuterHtml; //using this results in a page that displays my inserted HTML correctly, but duplicates the original page content.
                                                             //rawHtml = "some text"; uncommenting this results in a page with the correct format: this text, followed by the original contents of the page

            return rawHtml;
        }
    }
}