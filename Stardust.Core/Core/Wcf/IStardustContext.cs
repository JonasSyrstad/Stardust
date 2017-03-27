using System;

namespace Stardust.Core.Wcf
{
    public interface IStardustContext : IDisposable
    {
        Guid ContextId { get; }
        void SetDisconnectorAction(Action<object> action);
        void ClearDisposeActoion();
    }
}