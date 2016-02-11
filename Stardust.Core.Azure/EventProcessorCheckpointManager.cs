using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Stardust.Core.Azure
{
    public class EventProcessorCheckpointManager : ICheckpointManager
    {
        #region Public Properties
        public string Namespace { get; set; }
        public string EventHub { get; set; }
        #endregion


        #region ICheckpointManager Methods
        public Task CheckpointAsync(Lease lease, string offset, long sequenceNumber)
        {
            return EventProcessorCheckpointHelper.CheckpointAsync(Namespace, EventHub, lease, offset, sequenceNumber);
        }
        #endregion
    }
}