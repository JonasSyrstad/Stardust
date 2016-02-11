using System;
using System.Collections.Generic;
using Autofac;
using Stardust.Nucleus.ContainerIntegration;
using Stardust.Nucleus.ContextProviders;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Core.AutoFac
{
    internal class AutoFacResolverWrapper : IDependencyResolver
    {
        private readonly AutoFacKernelWrapper ConfigurationKernel;
        private IContainer Container;

        public AutoFacResolverWrapper(IConfigurationKernel configurationKernel)
        {
            ConfigurationKernel = (AutoFacKernelWrapper)configurationKernel;
            Container = ConfigurationKernel.GetAutoFacConfigItem().Build();
        }

        public T GetService<T>()
        {
            return Container.ResolveNamed<T>("default");
        }

        public T GetService<T>(Action<T> initializer)
        {
            return Container.ResolveNamed<T>("default");
        }

        public T GetService<T>(string named)
        {
            return Container.ResolveNamed<T>(named);
        }

        public T GetService<T>(string named, Action<T> initializer)
        {
            return Container.ResolveNamed<T>(named);
        }

        public T[] GetServices<T>()
        {
            throw new NotImplementedException();
        }

        public object GetService(Type serviceType, string named)
        {
            return Container.ResolveNamed(named, serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IExtendedScopeProvider BeginExtendedScope(IExtendedScopeProvider scope)
        {
            return new AutoFacExtendedScope(Container, scope);
        }
    }
}