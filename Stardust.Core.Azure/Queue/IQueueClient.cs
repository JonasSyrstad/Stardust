using Microsoft.ServiceBus.Messaging;

namespace Stardust.Core.Azure.Queue
{
    public interface IQueueClient
    {
        void Initialize(QueueClient queueClient);
    }
}