using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Net;
using Microsoft.ServiceBus.Messaging;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Messaging;
using Stardust.Particles;
using Stardust.Nucleus;

namespace Stardust.Core.Azure.Queue
{
    public class QueueSenderContainer<T> : IServiceContainer<T>
    {
        private readonly Endpoint endpoint;
        private IQueueSender client;
        private string url;

        public QueueSenderContainer(Endpoint endpoint)
        {
            this.endpoint = endpoint;
        }

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

        public IServiceContainer<T> Initialize(BootstrapContext bootstrapContext)
        {
            return this;
        }

        public IServiceContainer<T> SetCredentials(string username, string password)
        {
            return this;
        }

        public T GetClient()
        {
            if (!client.IsNull()) return (T)client;
            url = string.Format("{0}{1}", endpoint.PropertyBag["Address"], endpoint.PropertyBag["SenderKey"]);
            var clients = new List<QueueClient>();
            foreach (var datacenter in endpoint.PropertyBag["DataCenters"].Split('|'))
            {
                var queueClient = QueueClient.CreateFromConnectionString(url, string.Format(endpoint.PropertyBag["QueueName"], datacenter));
                clients.Add(queueClient);
            }
            client = Resolver.Activate<IQueueSender>(ServiceName,c => c.Initialize(clients.ToArray()));
            return (T)client;
        }

        public string ServiceName { get; internal set; }

        public void SetNettworkCredentials(NetworkCredential credential)
        {

        }

        public IServiceContainer<T> SetServiceRoot(string serviceRootUrl)
        {
            return this;
        }

        public string GetUrl()
        {
            return url;
        }

        public bool Initialized { get; private set; }
    }
}
