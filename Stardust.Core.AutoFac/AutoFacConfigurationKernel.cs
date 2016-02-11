using System;
using Autofac;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Core.AutoFac
{
    internal class AutoFacConfigurationKernel : IConfigurator
    {
        private readonly AutoFacKernelWrapper ConfigurationKernel;
        private ContainerBuilder Configuration;

        public AutoFacConfigurationKernel(IConfigurationKernel configurationKernel)
        {
            ConfigurationKernel = (AutoFacKernelWrapper) configurationKernel;
            Configuration = ConfigurationKernel.GetAutoFacConfigItem();
        }

        public IBindContext<T> Bind<T>()
        {
            return new AfBindContext<T>(Configuration);
        }

        public IBindContext BindAsGeneric(Type genericUnboundType)
        {
            throw new NotImplementedException();
        }

        public void RemoveAll()
        {
            throw new NotImplementedException();
        }

        public IUnbindContext<T> UnBind<T>()
        {
            return new AutoFacUnbindContext<T>(Configuration);
        }
    }
}