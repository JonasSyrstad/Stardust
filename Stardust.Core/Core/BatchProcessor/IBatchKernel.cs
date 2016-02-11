using System;
using System.Collections.Generic;

namespace Stardust.Core.BatchProcessor
{
    public interface IBatchKernel<T>
    {
        bool UpdateItem(string itemId, T withPayload,string source);

        T GetItem(string itemId, string source);

        bool InvalidateUpdateFlag(string itemId, string source);

        IEnumerable<string> GetModifiedItems(DateTime since);

        bool IsSynchronizerRunning(string name);

        ICoordinatedItemContainer GetItem(string itemId);
        void SetExportItem(string itemId, ICoordinatedItemContainer coordinationItem);

        void SetSynchronizationRunState(bool value, string name);
    }
}