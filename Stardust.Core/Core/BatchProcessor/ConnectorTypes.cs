using System;
using System.Collections.Generic;

namespace Stardust.Core.BatchProcessor
{
    public enum ConnectorTypes
    {
        Import,
        Export,
        Synchronize
    }

    public interface IKernelRepository
    {
        bool UpsertItem(string itemId, object withPayload, UpdateTypes updateTypes,string source);
        ICoordinatedItemContainer GetItemById(string itemId);
        IEnumerable<string> GetModifiedItems(DateTime since);
        void SetEnvironment(string name);

        void UpsertItem(string itemId, ICoordinatedItemContainer coordinationItem);
    }

    public interface ICoordinatedItemContainer
    {
        object MasterItem { get; set; }

        Dictionary<string, object> Sources { get; set; }
        DateTime LastUpdated { get; set; }
    }
}