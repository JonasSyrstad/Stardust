using System.Linq;
using Stardust.Interstellar;

namespace Stardust.Core.Azure.Queue
{
    public class QueueSenderContainerFactory : IServiceContainerFactory
    {
        public IServiceContainer<TService> CreateContainer<TService>(IRuntime runtime, string serviceName, Nucleus.Scope scope = Nucleus.Scope.Context) where TService : class
        {
            var settings = runtime.Context.GetEndpointConfiguration(serviceName).Endpoints.First();
            var container = new QueueSenderContainer<TService>(settings) {ServiceName = serviceName};
            return container;
        }
    }
}