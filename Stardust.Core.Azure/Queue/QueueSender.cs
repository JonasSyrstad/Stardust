using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Stardust.Interstellar;

namespace Stardust.Core.Azure.Queue
{
    [ServiceParameters("Address", "QueueName", "DataCenters", "ListenerKey", "SenderKey")]
    public abstract class QueueSender<T> : IQueueSender
    {
        private QueueClient[] clients;
        public event EventHandler<T> MessageReceived;

        void IQueueSender.Initialize(QueueClient[] queueClient)
        {
            clients = queueClient;
        }

        public void Send(T message)
        {
            foreach (var client in clients)
            {
                var queueMessage = new BrokeredMessage(message);
                client.Send(queueMessage);
            }
        }

        public Task SendAsync(T message)
        {
            var tasks = new List<Task>();
            foreach (var client in clients)
            {
                var queueMessage = new BrokeredMessage(message);
                tasks.Add(client.SendAsync(queueMessage));
            }
            return Task.WhenAll();
        }
    }
}