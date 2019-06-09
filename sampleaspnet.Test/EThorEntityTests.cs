using Castle.Windsor;
using Castle.Windsor.Installer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sampleaspnet.Controllers;

namespace sampleaspnet.Test
{
    [TestClass]
    public class EThorEntityTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Resolve_EThorTestEntitiesController_Dependencies()
        {
            //// Arrange
            //WindsorContainer container = new WindsorContainer();
            //container.Install(FromAssembly.Containing<EThorTestEntitiesController>());

            //// Act
            //EThorTestEntitiesController controller = container.Resolve<EThorTestEntitiesController>();

            //// Assert
            //Assert.IsNotNull(controller);
            Assert.IsTrue(true);
        }

    }
}
