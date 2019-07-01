using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using aspnetcoreoauth.Controllers;
using aspnetcoreoauth.Data;
using aspnetcoreoauth.Models;
using aspnetcoreoauth.Services;
using Castle.Windsor.MsDependencyInjection;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;

namespace aspnetcoreoauth.Tests
{
    public class SampleEntityTests
    {
        private SampleTestEntitiesController _SampleEntitiesController;
        private IApplicationDBContext _applicationDBContext;
        private ISampleEntityService _SampleEntityService;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();

            //services.AddTransient<IConfiguration>(provider => PopulateTestData.GetConfiguration(TestContext.CurrentContext.TestDirectory));
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase());
            services.AddTransient<IApplicationDBContext, ApplicationDbContext>();
            //services.AddTransient<ISampleEntityService, SampleEntityService>();
            //services.AddTransient<SampleTestEntitiesController>();

            //var serviceProvider = services.BuildServiceProvider();
            //_applicationDBContext = serviceProvider.GetService<IApplicationDBContext>();
            //_SampleEntityService = serviceProvider.GetService<ISampleEntityService>();
            //_SampleEntitiesController = serviceProvider.GetService<SampleTestEntitiesController>();


            var container = new WindsorContainer();
            var assemblyName = typeof(aspnetcoreoauth.Startup).Assembly.GetName().Name;
            container.Register(Component.For<IApplicationDBContext, ApplicationDbContext>().LifestyleTransient());
            container
                .Register(
                    Component.For<ISampleEntityService, SampleEntityService>()
                    .LifestyleTransient());
            container
                .Register(
                    Classes.FromAssemblyNamed(assemblyName).Pick().If(p => p.Name.EndsWith("Controller"))
                    .LifestyleTransient());

            var serviceProvider = WindsorRegistrationHelper.CreateServiceProvider(container, services);
            _applicationDBContext = container.Resolve<IApplicationDBContext>();
            _SampleEntitiesController = container.Resolve<SampleTestEntitiesController>();
            _SampleEntityService = container.Resolve<ISampleEntityService>();
        }

        [Test, Order(1)]
        public void Check_for_resolved_dependencies()
        {
            Assert.IsNotNull(_applicationDBContext);
            Assert.IsNotNull(_SampleEntityService);
            Assert.IsNotNull(_SampleEntitiesController);
        }

        [Test, Order(2)]
        public void Add_SampleTestEntity()
        {
            var newEntity = new SampleTestEntity() { Name = "test entity", HardProperty = new string[] { "testA", "testB" } };
            int? returnId =  _SampleEntityService.AddSampleTestEntity(newEntity).GetAwaiter().GetResult();

            Assert.IsNotNull(returnId, "Entity was not added");
            Assert.Greater(returnId,0, "Entity count was not updated");
        }

        [Test, Order(3)]
        public void Get_SampleTestEntity()
        {
            var newEntity = new SampleTestEntity() { Name = "test entity", HardProperty = new string[] { "testA", "testB" } };
            int? returnId = _SampleEntityService.AddSampleTestEntity(newEntity).GetAwaiter().GetResult();
            Assert.IsNotNull(returnId, "Entity was not added");

            var result = _SampleEntityService.GetSampleTestEntity(1).GetAwaiter().GetResult();
            Assert.IsNotNull(result, "Entity was not avaialable and resulted as null");
            Assert.IsInstanceOf<SampleTestEntity>(result, "Entity is not of type SampleTestEntity");
        }

        [Test, Order(4)]
        public void Update_SampleTestEntity()
        {
            var newEntity = new SampleTestEntity() { Name = "test entity", HardProperty = new string[] { "testA", "testB" } };
            int? returnId = _SampleEntityService.AddSampleTestEntity(newEntity).GetAwaiter().GetResult();
            Assert.IsNotNull(returnId, "Addition of entity caused error");

            newEntity.HardProperty = new string[] { "testA", "testB", "testC" };
            int? returnUpdateId = _SampleEntityService.UpdateSampleTestEntity(newEntity).GetAwaiter().GetResult();
            Assert.IsNotNull(returnUpdateId, "Entity could not be updated");
            Assert.Greater(returnUpdateId, 0, "Entity update count was not as expected");

            var resultEntity = _SampleEntityService.GetSampleTestEntity(1).GetAwaiter().GetResult();
            Assert.IsNotNull(resultEntity, "Entity was not avaialable and resulted as null");
            Assert.AreEqual(resultEntity.HardProperty.Count(),3, "HardProperty item count was not as expected");
        }

        [Test, Order(5)]
        public void Delete_SampleTestEntity()
        {
            var newEntity = new SampleTestEntity() { Name = "test entity", HardProperty = new string[] { "testA", "testB" } };
            int? returnId = _SampleEntityService.AddSampleTestEntity(newEntity).GetAwaiter().GetResult();
            Assert.IsNotNull(returnId, "Addition of entity caused error");

            var result = _SampleEntityService.DeleteSampleTestEntity(newEntity.Id).GetAwaiter().GetResult();
            Assert.IsNotNull(result, "Entity could not be deleted");
            Assert.Greater(result, 0, "Entity deletion was not as expected");
        }

        [Test, Order(6)]
        public void GetAll_TestEntity()
        {
            var newEntity1 = new SampleTestEntity() { Name = "test entity 1", HardProperty = new string[] { "test1A", "test1B" } };
            int? returnId1 = _SampleEntityService.AddSampleTestEntity(newEntity1).GetAwaiter().GetResult();
            Assert.IsNotNull(returnId1, "Addition of 2nd entity caused error");

            var newEntity2 = new SampleTestEntity() { Name = "test entity 2", HardProperty = new string[] { "test2A", "test2B" } };
            int? returnId2 = _SampleEntityService.AddSampleTestEntity(newEntity2).GetAwaiter().GetResult();
            Assert.IsNotNull(returnId2, "Addition of 2nd entity caused error");

            var result = _SampleEntityService.GetSampleTestEntityList(e => e.Id != null).GetAwaiter().GetResult();
            Assert.IsNotNull(result, "Entity list was not available");
            Assert.AreEqual(result.Count, 2, "Entity list was not as expected");
        }
    }
}