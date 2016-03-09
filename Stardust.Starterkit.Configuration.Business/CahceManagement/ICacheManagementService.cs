using System;
using Stardust.Interstellar;

namespace Stardust.Starterkit.Configuration.Business.CahceManagement
{
    public interface ICacheManagementService : IRuntimeTask
    {
        bool TryUpdateCache(string configSet, string environment);

        void RegisterRealtimeNotificationService(Action<string, string> action);
    }
}
