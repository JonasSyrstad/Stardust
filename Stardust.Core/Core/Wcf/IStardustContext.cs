using System;

namespace Stardust.Core.Wcf
{
    public interface IStardustContext : IDisposable
    {
        Guid ContextId { get; }
        event EventHandler<EventArgs> Disposing;
    }
}