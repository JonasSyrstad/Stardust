using Stardust.Nucleus.ContainerIntegration;
using Stardust.Nucleus.Internals;

namespace Stardust.Nucleus.TypeResolver
{
    public class StardustIocFactory : IContainerSetup
    {
        public IConfigurator GetConfigurator(IConfigurationKernel configurationKernel)
        {
            return new Configurator(configurationKernel);
        }

        public IConfigurationKernel GetKernel()
        {
            return new TypeResolverConfigurationKernel(new TypeLocatorOptimizer(), new TypeResolverFromConfiguration(),new TypeResolverFromAssemblyScanning());
        }

        public IDependencyResolver GetResolver(IConfigurationKernel configurationKernel)
        {
            return new StardustDependencyResolver(() => configurationKernel);
        }
    }
}