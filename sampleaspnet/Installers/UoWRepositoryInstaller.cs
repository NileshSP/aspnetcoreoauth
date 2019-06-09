using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using sampleaspnet.Data;
using sampleaspnet.Models;
//using CoreIdentity.Core.Data;
//using CoreIdentity.Core.Data.Imp;
//using CoreIdentity.Data;

namespace sampleaspnet
{
    public class UoWRepositoryInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            //container.Register(
            //    Component.For(typeof(IUnitOfWork))
            //    .ImplementedBy(typeof(UnitOfWork)));
            //container.Register(
            //    Component.For(typeof(IEntitiesContext))
            //    .ImplementedBy(typeof(BloggingContext)));
            container.Register(
                Component.For(typeof(IApplicationDBContext))
                .ImplementedBy(typeof(ApplicationDbContext))
                .LifestyleTransient()
                );
        }
    }
}
