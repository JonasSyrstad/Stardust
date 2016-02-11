using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Nucleus;
using System.Linq;

namespace Stardust.Core.Azure
{
    public abstract class EventHubServiceContainerFactory<T> : IServiceContainerFactory
    {
        public IServiceContainer<TService> CreateContainer<TService>(IRuntime runtime, string serviceName, Scope scope = Scope.Context)
        {
            var container = new NotificationSenderContainer<TService>();
            var endpoint = GetEndpointSettings(runtime.Context, serviceName);
            container.SetServiceRoot(string.Format("{0}{1}", endpoint.PropertyBag["Address"], endpoint.PropertyBag["SenderKey"]));
            container.SetContext(runtime.Context, serviceName);
            return container;
        }

        private Endpoint GetEndpointSettings(IRuntimeContext settings, string serviceName)
        {
            var serviceSettings = (from i in settings.GetEndpointConfiguration(serviceName).Endpoints
                                   where i.EndpointName == settings.GetEndpointConfiguration(serviceName).ActiveEndpoint
                                   select i).Single();
            return serviceSettings;
        }
    }
}