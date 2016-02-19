using System;
using System.Collections.Generic;
using System.Linq;
using Stardust.Frontier.BatchProcessor;

namespace Stardust.Core.CrossCuttingTest.LegacyTests.BatchCoordinator
{
    public class MemoryRepository : IKernelRepository
    {
        internal static Dictionary<string, ICoordinatedItemContainer> Repository = new Dictionary<string, ICoordinatedItemContainer>();
        public bool UpsertItem(string itemId, object withPayload, UpdateTypes updateTypes, string source)
        {
            try
            {
                if (updateTypes == UpdateTypes.Unchanged)
                    return true;
                ICoordinatedItemContainer item;
                if (!Repository.TryGetValue(itemId, out item))
                {
                    item = new CoordiantionItemContainer { Sources = new Dictionary<string, object>() };
                    Repository.Add(itemId, item);
                }
                object data;
                if (item.Sources.TryGetValue(source, out data))
                    item.Sources.Remove(itemId);
                item.Sources.Add(source, withPayload);
                item.LastUpdated = DateTime.UtcNow;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ICoordinatedItemContainer GetItemById(string itemId)
        {
            ICoordinatedItemContainer item;
            return Repository.TryGetValue(itemId, out item) ? item : null;
        }

        public IEnumerable<string> GetModifiedItems(DateTime since)
        {
            return (from i in Repository where i.Value.LastUpdated >= since select i.Key).ToList();
        }

        public void SetEnvironment(string name)
        {

        }

        public void UpsertItem(string itemId, ICoordinatedItemContainer coordinationItem)
        {

        }
    }
}