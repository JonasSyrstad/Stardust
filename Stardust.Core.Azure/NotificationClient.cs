using System;
using System.IO;
using Microsoft.ServiceBus.Messaging;
using Stardust.Particles;

namespace Stardust.Core.Azure
{
    public abstract class NotificationClient<T> : INotificationClient, IDisposable
    {
        protected virtual T DeserializeMessage(Stream message)
        {
            return SerializationHelpers.Deserializer<T>(message);
        }

        internal static T DoDeserializeMessage(Stream message)
        {
            return self.DeserializeMessage(message);
        }

        protected NotificationClient()
        {
            self = this;
        }

        private EventHubReceiver[] Readers;
        public static NotificationClient<T> self;
        public static void RaiseEvent(T message)
        {
            if (self.MessageReceived != null)
                self.MessageReceived(self, message);
        }

        private string Offset;
        private bool Disposed;
        private EventHubConsumerGroup Consumer;
        private string[] PartitionIds;

        public void Initialize(EventHubConsumerGroup sender, string[] partitionIds)
        {
            Consumer = sender;
            PartitionIds = partitionIds;
            foreach (var item in partitionIds)
            {
                sender.RegisterProcessor<NotificationProcessor<T>>(EventProcessorCheckpointHelper.GetLease(GetType().Name, sender.EventHubPath, item), new EventProcessorCheckpointManager());
            }

        }

        public event EventHandler<T> MessageReceived;

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            Disposed = true;
            if (MessageReceived != null)
            {
                Logging.DebugMessage("there are connected clients....");
                MessageReceived = null;
            }
            ShutdownEventPump();
            DoDispose();
        }

        protected virtual void DoDispose()
        {
        }

        private void ShutdownEventPump()
        {
            try
            {
                foreach (var partitionId in PartitionIds)
                {
                    Logging.DebugMessage(string.Format("Shutting down processor {0}", partitionId));
                    Consumer.UnregisterProcessor(EventProcessorCheckpointHelper.GetLease(GetType().Name, Consumer.EventHubPath, partitionId),CloseReason.Shutdown);
                }
                Consumer.Close();
                Consumer = null;
                Logging.DebugMessage("Shutdown completed");
            }
            catch (Exception ex)
            {
                Logging.Exception(ex);
            }
        }

        ~NotificationClient()
        {
            Dispose(false);
        }
    }
}