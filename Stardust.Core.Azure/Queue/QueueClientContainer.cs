using System;
using System.IdentityModel.Tokens;
using System.Net;
using Microsoft.ServiceBus.Messaging;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Messaging;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Core.Azure.Queue
{
    public class QueueClientContainer<T> : IServiceContainer<T>
    {
        private readonly Endpoint endpoint;
        private IQueueClient client;
        private string url;

        internal QueueClientContainer(Endpoint endpoint)
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

            if (client.IsNull())
            {
                url = string.Format("{0}{1}",endpoint.PropertyBag["Address"], endpoint.PropertyBag["ListenerKey"]);
                var queueClient = QueueClient.CreateFromConnectionString(url, String.Format(endpoint.PropertyBag["QueueName"], ConfigurationManagerHelper.GetValueOnKey("dataCenterName")));
                client = Resolver.Activate<IQueueClient>(ServiceName);
                client.Initialize(queueClient);
            }
            return (T) client;
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