using System;
using System.Collections.Concurrent;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    /// <summary>
    /// Used to register service container factories. Registering a service container factory allows you to wrap a messaging technology behind the Stardust framework.
    /// </summary>
    public static class ServiceContainerFactory
    {
        public static void ResetFactory()
        {
            ImplementationCache.Clear();
            DefaultServiceContainerFactory=new WcfServiceContainerFactory();
            DefaultPooledServiceContainerFactory=new WcfServiceContainerFactory();
        }

        private static IPooledServiceContainerFactory DefaultPooledServiceContainerFactory = new WcfServiceContainerFactory();

        private static readonly ConcurrentDictionary<Type, IServiceContainerFactory> ImplementationCache = new ConcurrentDictionary<Type, IServiceContainerFactory>();

        private static IServiceContainerFactory DefaultServiceContainerFactory = new WcfServiceContainerFactory();

        /// <summary>
        /// Registers an instance of the <see cref="IServiceContainerFactory"/> to use with TService 
        /// </summary>
        /// <param name="factory">the factory instance</param>
        /// <typeparam name="TService">the service that will be created with the factory</typeparam>
        public static void RegisterServiceFactory<TService>(IServiceContainerFactory factory)
        {
            var item = GetFactory<TService>();
            if (item.IsNull())
            {
                ImplementationCache.TryAdd(typeof(TService), factory);
            }
        }

        /// <summary>
        /// Registers a Service factory
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static ServiceFactoryBindContext<TService> RegisterService<TService>()
        {
            return new ServiceFactoryBindContext<TService>();
        }

        internal static IServiceContainer<TService> CreateContainer<TService>(IRuntime runtime, string serviceName, Scope scope = Scope.Context) where TService : class
        {
            var alternateContainerFactory = GetFactory<TService>();
            if (alternateContainerFactory.IsInstance())
                return alternateContainerFactory.CreateContainer<TService>(runtime, serviceName, scope);
            return DefaultServiceContainerFactory.CreateContainer<TService>(runtime, serviceName, scope);
        }

        internal static IPooledServiceContainer<TService> CreatePooledServiceContainer<TService>(IRuntime runtime) where TService : class
        {
            return DefaultPooledServiceContainerFactory.CreatePooledServiceProxy<TService>(runtime);
        }

        internal static IPooledServiceContainer<TService> CreatePooledServiceProxy<TService>(IRuntime runtime, string serviceName) where TService : class
        {
            return DefaultPooledServiceContainerFactory.CreatePooledServiceProxy<TService>(runtime, serviceName);
        }

        public static void RegisterServiceFactoryAsDefault(IServiceContainerFactory factory)
        {
            DefaultServiceContainerFactory = factory;
        }

        private static IServiceContainerFactory GetFactory<T>()
        {
            IServiceContainerFactory implementation;
            return ImplementationCache.TryGetValue(typeof(T), out implementation) ? implementation : null;
        }
    }


}