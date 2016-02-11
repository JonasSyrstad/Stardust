using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using Microsoft.ServiceBus.Messaging;
using Stardust.Interstellar;
using Stardust.Interstellar.Messaging;
using Stardust.Particles;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Nucleus;

namespace Stardust.Core.Azure
{
    public class NotificationSenderContainer<T> : IServiceContainer<T>
    {
        private string ServiceRoot;
        private T Sender;
        private IRuntimeContext Settings;
        private string ServiceName;

        public void Dispose()
        {

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

        private Endpoint GetEndpointSettings(string serviceName)
        {
            var serviceSettings = (from i in Settings.GetEndpointConfiguration(serviceName).Endpoints
                                   where i.EndpointName == Settings.GetEndpointConfiguration(serviceName).ActiveEndpoint
                                   select i).Single();
            return serviceSettings;
        }

        public T GetClient()
        {
            if (Sender.IsInstance())
                return Sender;
            var notificationHub = EventHubClient.CreateFromConnectionString(ServiceRoot, GetEndpointSettings(ServiceName).PropertyBag["PublisherName"]);
            var sender = notificationHub.CreateSender(GetEndpointSettings(ServiceName).PropertyBag["PublisherName"]);
            Sender = (T)Resolver.Activate<INotificationService>(ServiceName,s => s.Initialize(sender));
            return Sender;
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