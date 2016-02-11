using Stardust.Nucleus;
using Stardust.Nucleus.ContainerIntegration;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Core.AutoFac
{
    public class AutoFacFactory : IContainerSetup
    {
        public IConfigurator GetConfigurator(IConfigurationKernel configurationKernel)
        {
            return new AutoFacConfigurationKernel(configurationKernel);
        }

        public IConfigurationKernel GetKernel()
        {
            return new AutoFacKernelWrapper();
        }

        public IDependencyResolver GetResolver(IConfigurationKernel configurationKernel)
        {
            return new AutoFacResolverWrapper(configurationKernel);
        }
    }
}
