using System;
using System.Collections.Generic;
using System.ServiceModel;
using Stardust.Core.Pool;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    /// <summary>
    /// An creates and configures wcf based service containers.
    /// </summary>
    public sealed class WcfServiceContainerFactory : IServiceContainerFactory, IPooledServiceContainerFactory
    {
        private static Dictionary<string, bool> PoolStatus = new Dictionary<string, bool>();

        /// <summary>
        /// Creates a configured instance of a WCF service container. 
        /// </summary>
        /// <typeparam name="TService">The service to wrap in a service container</typeparam>
        /// <param name="runtime">The current <see cref="IRuntime"/> instance</param>
        /// <param name="serviceName">The service name for lookup in the configuration system</param>
        /// <param name="scope">the OLM scope for the created instance.</param>
        /// <returns></returns>
        public IServiceContainer<TService> CreateContainer<TService>(IRuntime runtime, string serviceName, Scope scope = Scope.Context)
        {
            var serviceContainer = (ServiceContainer<TService>)ContainerFactory.Current.Resolve(typeof(ServiceContainer<TService>), scope, GetServiceContainer<TService>);
            if (serviceContainer.Initialized) return serviceContainer;
            var attrib = typeof(TService).GetAttribute<ServiceNameAttribute>();
            var serviceRootUrl = attrib.IsInstance()
                ? runtime.Context.GetEnvironmentConfiguration().GetConfigParameter(attrib.UrlRootName)
                : null;
            serviceContainer.InitializeContainer(CreateChannelFactory<TService>(serviceName, runtime.Context), runtime.Context.GetEndpointConfiguration(serviceName).GetRemoteAddress(serviceRootUrl), serviceName);
            serviceContainer.SetServiceRoot(serviceRootUrl);
            return serviceContainer;
        }

        private static ServiceContainer<TService> GetServiceContainer<TService>()
        {
            var serviceContainer = new ServiceContainer<TService>();
            return serviceContainer;
        }
        private static ChannelFactory<TService> CreateChannelFactory<TService>(string serviceName, IRuntimeContext context)
        {
            var service = context.GetEndpointConfiguration(serviceName);
            var endpoint = service.GetEndpoint(service.ActiveEndpoint);
            var isSecureRest = endpoint.EndpointName.Equals("securerest", StringComparison.OrdinalIgnoreCase);
            
            return ServiceProxyBuilder.CreateChannelFactory<TService>(context, serviceName, context.GetEndpointConfiguration(serviceName).GetRemoteAddress(),isSecureRest);
        }

        public IPooledServiceContainer<TService> CreatePooledServiceProxy<TService>(IRuntime runtime)
        {
            var name = Utilities.Utilities.GetServiceNameFromAttribute(typeof(TService));
            if (name.IsInstance()) return CreatePooledServiceProxy<TService>(runtime, name.ServiceName);
            return CreatePooledServiceProxy<TService>(runtime, runtime.Context.GetClientProxyBindingName<TService>());
        }

        public IPooledServiceContainer<TService> CreatePooledServiceProxy<TService>(IRuntime runtime, string serviceName)
        {
            var attrib = typeof(TService).GetAttribute<ServiceNameAttribute>();
            var serviceRootUrl = attrib.IsInstance()
                ? runtime.Context.GetEnvironmentConfiguration().GetConfigParameter(attrib.UrlRootName)
                : null;
            var address = runtime.Context.GetEndpointConfiguration(serviceName).GetRemoteAddress(serviceRootUrl);
            if (!PoolStatus.ContainsKey(address))
            {
                lock (PoolStatus)
                {
                    if (!PoolStatus.ContainsKey(address))
                    {
                        PoolFactory.InitializeNamedPool<PooledServiceContainer<TService>>(GetInstanceCount(runtime.Context), (i) => InitializePoolItem<TService>(i, serviceName, serviceRootUrl, runtime.Context));
                        PoolStatus.Add(address, true);
                    }
                }
            }
            return PoolFactory.Create<PooledServiceContainer<TService>>(address);
        }

        private static PooledServiceContainer<T> InitializePoolItem<T>(PooledServiceContainer<T> serviceContainer, string serviceName, string serviceRootUrl, IRuntimeContext context)
        {
            if (!serviceContainer.Initialized)
            {
                serviceContainer.InitializeContainer(CreateChannelFactory<T>(serviceName, context), serviceName);
                serviceContainer.SetServiceRoot(serviceRootUrl);
            }
            return serviceContainer;
        }

        private static int GetInstanceCount(IRuntimeContext Context)
        {
            var value = Context.GetServiceConfiguration().GetConfigParameter("SerivceProxyPoolSize");
            if (value.ContainsCharacters())
                return int.Parse(value);
            return 1;
        }
    }
}