using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business.CahceManagement
{
    public class NullCacheManagementWrapper : AbstractRuntimeTask, ICacheManagementWrapper
    {
        public NullCacheManagementWrapper(IRuntime runtime)
            : base(runtime)
        {
        }

        public bool UpdateCache(IEnvironment environmentSettings, ConfigurationSet raw)
        {
            return true;
        }
    }
}