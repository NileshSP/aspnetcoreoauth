using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace sampleaspnet
{
    public class ControllerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /*container.Register(
                Classes.FromAssemblyNamed("CoreIdentity").Pick().If(p => p.Name.EndsWith("Controller"))
                .LifestyleTransient());*/
            //.Configure(component => component.Interceptors<LoggingInterceptor>()));
            container.Register(
                Classes.FromAssemblyNamed("sampleaspnet").Pick().If(p => p.Name.EndsWith("Controller"))
                .LifestyleTransient());
        }
    }
}
