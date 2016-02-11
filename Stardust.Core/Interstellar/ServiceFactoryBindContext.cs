using Stardust.Nucleus;

namespace Stardust.Interstellar
{
    /// <summary>
    /// Binding context for the service
    /// </summary>
    /// <typeparam name="TService">the service contract</typeparam>
    public class ServiceFactoryBindContext<TService>
    {
        internal ServiceFactoryBindContext()
        {
        }

        /// <summary>
        /// Create the binding within the IOC container
        /// </summary>
        /// <typeparam name="TServiceFactory">the type of factory to use with <typeparamref name="TService"/></typeparam>
        public ServiceFactoryResolveContext<TService, TServiceFactory> WithFactory<TServiceFactory>() where TServiceFactory : IServiceContainerFactory
        {
            Resolver.GetConfigurator().Bind<IServiceContainerFactory>().To<TServiceFactory>(typeof(TService).FullName).SetTransientScope();
            return new ServiceFactoryResolveContext<TService, TServiceFactory>();

        }

        /// <summary>
        /// Sets the implementation to the factory cache
        /// </summary>
        public class ServiceFactoryResolveContext<TService, TFactory> where TFactory : IServiceContainerFactory
        {
            internal ServiceFactoryResolveContext()
            {
                
            }
            public ServiceFactoryResolveContext<TService, TFactory> Resolve()
            {
                ServiceContainerFactory.RegisterServiceFactory<TService>(Resolver.Activate<IServiceContainerFactory>(typeof(TService).FullName));
                return this;
            }

            public ServiceFactoryResolveContext<TService, TFactory> SetAsDefault()
            {
                ServiceContainerFactory.RegisterServiceFactoryAsDefault(Resolver.Activate<IServiceContainerFactory>(typeof(TService).FullName));
                return this;
            }
        }
    }
}