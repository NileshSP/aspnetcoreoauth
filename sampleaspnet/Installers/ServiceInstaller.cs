using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using sampleaspnet.Models;
using sampleaspnet.Services;

namespace sampleaspnet
{
    public class ServiceInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            //container.Register(
            //    Component.For<IEmailSender>()
            //     .ImplementedBy<EmailSender>()
            //     .Interceptors<LoggingInterceptor>()); 
            //container.Register(
            //    Component.For(typeof(IService<>))
            //    .ImplementedBy(typeof(Service<>)));
            container.Register(
                Component.For(typeof(IEThorEntityService))
                .ImplementedBy(typeof(EThorEntityService)).LifestyleTransient());
        }
    }
}
