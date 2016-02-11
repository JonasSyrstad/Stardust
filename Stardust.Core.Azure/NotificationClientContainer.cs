using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using Microsoft.ServiceBus.Messaging;
using Stardust.Nucleus;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Messaging;
using Stardust.Particles;

namespace Stardust.Core.Azure
{
    public class NotificationClientContainer<T> : IServiceContainer<T>
    {
        private string ServiceRoot;
        private T Client;
        private EventHubClient NotificationHub;
        private IRuntimeContext Settings;
        private string ServiceName;

        public void Dispose()
        {
            Dispose(true);
        }

        private Endpoint GetEndpointSettings(string serviceName)
        {
            var serviceSettings = (from i in Settings.GetEndpointConfiguration(serviceName).Endpoints
                                   where i.EndpointName == Settings.GetEndpointConfiguration(serviceName).ActiveEndpoint
                                   select i).Single();
            return serviceSettings;
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            Client.TryDispose();
            Client = default(T);
            NotificationHub.Close();
            NotificationHub = null;
        }

        ~NotificationClientContainer()
        {
            Dispose(false);
        }

        public TOut ExecuteMethod<TOut>(Func<TOut> executor) where TOut : IResponseBase
        {
            throw new NotImplementedException();
        }

        public IServiceContainer<T> Initialize(bool useSecure = false)
        {
            return this;
        }

        public IServiceContainer<T> SetCredentials(string username, string password)
        {
            return this;
        }

        public T GetClient()
        {
            if (Client.IsInstance())
                return Client;
            var serviceSettings = GetEndpointSettings();
            NotificationHub = EventHubClient.CreateFromConnectionString(ServiceRoot, GetEndpointSettings(ServiceName).PropertyBag["PublisherName"]);
            var receiver = serviceSettings.PropertyBag["SenderGroup"] == "default" ? NotificationHub.GetDefaultConsumerGroup() : NotificationHub.GetConsumerGroup(serviceSettings.PropertyBag["SenderGroup"]);
            Client = (T)Resolver.Activate<INotificationClient>(ServiceName,s => s.Initialize(receiver, NotificationHub.GetRuntimeInformation().PartitionIds));
            return Client;
        }

        private Endpoint GetEndpointSettings()
        {
            var serviceSettings = (from i in Settings.GetEndpointConfiguration(ServiceName).Endpoints
                where i.EndpointName == Settings.GetEndpointConfiguration(ServiceName).ActiveEndpoint
                select i).Single();
            return serviceSettings;
        }

        public void SetNettworkCredentials(NetworkCredential credential)
        {

        }

        public IServiceContainer<T> SetServiceRoot(string serviceRootUrl)
        {
            ServiceRoot = serviceRootUrl;
            return this;
        }

        public string GetUrl()
        {
            return "";
        }

        public bool Initialized
        {
            get { return true; }
        }

        public IServiceContainer<T> Initialize(BootstrapContext bootstrapContext)
        {
            return this;
        }

        public void SetContext(IRuntimeContext context, string serviceName)
        {
            Settings = context;
            ServiceName = serviceName;
        }
    }
}