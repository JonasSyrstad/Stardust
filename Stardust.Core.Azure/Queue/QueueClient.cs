using System;
using Microsoft.ServiceBus.Messaging;
using Stardust.Interstellar;
using Stardust.Particles;

namespace Stardust.Core.Azure.Queue
{
        [ServiceParameters("Address", "QueueName", "DataCenters", "ListenerKey", "SenderKey")]
    public abstract class QueueClient<T> : IQueueClient
    {
            
        private QueueClient client;
        public event EventHandler<T> MessageReceived;

        void IQueueClient.Initialize(QueueClient queueClient)
        {
            client = queueClient;
            client.OnMessage(m =>
            {
                if (!MessageReceived.IsInstance())
                {
                    m.Abandon();
                    return;
                }
                try
                {
                    MessageReceived(this, m.GetBody<T>());
                    m.Complete();
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex,"Message handling failed");
                    m.Abandon();
                }
            });
        }
    }
}