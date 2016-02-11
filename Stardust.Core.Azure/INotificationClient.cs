using Microsoft.ServiceBus.Messaging;

namespace Stardust.Core.Azure
{
    public interface INotificationClient
    {
        void Initialize(EventHubConsumerGroup sender, string[] partitionIds);
    }
}