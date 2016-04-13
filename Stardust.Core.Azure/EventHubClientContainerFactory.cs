using System.Linq;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Nucleus;

namespace Stardust.Core.Azure
{
    public abstract class EventHubClientContainerFactory<T> : IServiceContainerFactory
    {
        public IServiceContainer<TService> CreateContainer<TService>(IRuntime runtime, string serviceName,Scope scope = Scope.Context) where TService : class
        {
            var container= new NotificationClientContainer<TService>();
            var endpoint = GetEndpointSettings(runtime.Context, serviceName);
            container.SetServiceRoot(string.Format("{0}{1}",endpoint.PropertyBag["Address"],endpoint.PropertyBag["ListenerKey"]));
            container.SetContext(runtime.Context,serviceName);
            return container;
        }

        private Endpoint GetEndpointSettings(IRuntimeContext settings,string serviceName)
        {
            var serviceSettings = (from i in settings.GetEndpointConfiguration(serviceName).Endpoints
                                   where i.EndpointName == settings.GetEndpointConfiguration(serviceName).ActiveEndpoint
                                   select i).Single();
            return serviceSettings;
        }
    }
}   