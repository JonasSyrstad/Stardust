using System;
using System.Collections.Generic;
using System.Linq;
using Stardust.Nucleus.ContextProviders;
using Stardust.Nucleus.Internals;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Nucleus.ContainerIntegration
{
    /// <summary>
    /// Provides a common contract for integrating DI controllers into Stardust
    /// </summary>
    public interface IDependencyResolver
    {
        T GetService<T>();
        T GetService<T>(Action<T> initializer);

        T GetService<T>(string named);
        T GetService<T>(string named, Action<T> initializer);


        T[] GetServices<T>();

        object GetService(Type serviceType, string named);

        IEnumerable<object> GetServices(Type serviceType);
        IExtendedScopeProvider BeginExtendedScope(IExtendedScopeProvider scope);
        Dictionary<string, T> GetServicesNamed<T>(string exceptWithName);
        Dictionary<string, object> GetServicesNamed(Type serviceType);
    }

    internal sealed class StardustDependencyResolver : IDependencyResolver
    {
        internal StardustDependencyResolver(Func<IConfigurationKernel> kernel)
        {
            KernelResolver = kernel;
        }

        private Func<IConfigurationKernel> KernelResolver;

        private IResolveContext<T> FindServiceImplementation<T>(string named)
        {
            return new ResolveContext<T>((IScopeContextInternal)KernelResolver().Resolve(typeof(T), named));
        }

        public T GetService<T>()
        {
            return FindServiceImplementation<T>(TypeLocatorNames.DefaultName).Activate();
        }

        public T GetService<T>(Action<T> initializer)
        {
            return FindServiceImplementation<T>(TypeLocatorNames.DefaultName).SetInitializer(initializer).Activate();
        }

        public T GetService<T>(string named)
        {
            return FindServiceImplementation<T>(named).Activate();
        }

        public T GetService<T>(string named, Action<T> initializer)
        {
            return FindServiceImplementation<T>(named).SetInitializer(initializer).Activate();
        }

        public T[] GetServices<T>()
        {
            return KernelResolver().ResolveAll(typeof(T)).ActivateAll<T>();

        }

        public object GetService(Type serviceType, string named)
        {
            return KernelResolver().Resolve(serviceType, named).Activate();
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return (from i in KernelResolver().ResolveAll(serviceType) select i.Activate()).ToList();
        }

        public IExtendedScopeProvider BeginExtendedScope(IExtendedScopeProvider scope)
        {
            return scope;
        }

        public Dictionary<string, T> GetServicesNamed<T>(string exceptWithName)
        {
            if (exceptWithName == null)
                return (from i in KernelResolver().ResolveAllNamed(typeof(T)) select new { Instance = (T)i.Value.Activate(), Name = i.Key ?? "default" }).ToDictionary(k => k.Name, v => v.Instance);
            return (from i in KernelResolver().ResolveAllNamed(typeof(T)) where i.Key!=exceptWithName select new { Instance = (T)i.Value.Activate(), Name = i.Key ?? "default" }).ToDictionary(k => k.Name, v => v.Instance);
        }

        public Dictionary<string, object> GetServicesNamed(Type serviceType)
        {
            return (from i in KernelResolver().ResolveAllNamed(serviceType) select new { Instance = i.Value.Activate(), Name = i.Key ?? "default" }).ToDictionary(k => k.Name, v => v.Instance);
        }
    }

    internal static class ScopeContextExtensions
    {
        internal static T[] ActivateAll<T>(this IEnumerable<IScopeContext> contexts)
        {
            var list = new List<T>();
            foreach (var scopeContext in contexts)
            {
                list.Add((T)scopeContext.Activate());
            }
            return list.ToArray();
        }
    }
}
