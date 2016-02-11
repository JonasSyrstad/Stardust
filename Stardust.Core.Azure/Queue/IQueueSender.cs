using Microsoft.ServiceBus.Messaging;

namespace Stardust.Core.Azure.Queue
{
    public interface IQueueSender
    {
        void Initialize(QueueClient[] queueClient);
    }
}