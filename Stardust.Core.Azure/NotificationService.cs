using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Stardust.Interstellar;

namespace Stardust.Core.Azure
{
    [ServiceParameters("Address", "SenderKey","ListenerKey", "SenderGroup", "PublisherName")]
    public abstract class NotificationService<T> : INotificationService
    {
        private string Offset;
        private EventHubSender Sender;

        public void Initialize(EventHubSender sender)
        {
            Sender = sender;
        }

        public Task SendBatch(IEnumerable<T> messages)
        {
            return Sender.SendBatchAsync(messages.Select(CreateEventData));
        }

        public Task Send(T message)
        {
            return Sender.SendAsync(CreateEventData(message));
        }

        private EventData CreateEventData(T message)
        {
            return new EventData(SerializeMessage(message));
        }

        protected virtual MemoryStream SerializeMessage(T message)
        {
            return SerializationHelpers.SerializeToMemoryStream(message);
        }
    }
}