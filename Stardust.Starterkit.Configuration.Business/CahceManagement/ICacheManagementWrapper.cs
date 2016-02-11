using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business.CahceManagement
{
    /// <summary>
    /// Handles communication with the cache server in the client applications
    /// </summary>
    public interface ICacheManagementWrapper : IRuntimeTask
    {
        bool UpdateCache(IEnvironment environmentSettings, ConfigurationSet raw);
    }
}