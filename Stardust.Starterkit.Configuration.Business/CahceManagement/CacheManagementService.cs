using System;
using System.Linq;
using Stardust.Interstellar;
using Stardust.Interstellar.Trace;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business.CahceManagement
{
    public class CacheManagementService : AbstractRuntimeTask, ICacheManagementService
    {
        private IConfigSetTask Repository;

        private static Action<string, string> realtimeNotificationService;

        public CacheManagementService(IRuntime runtime)
            : base(runtime){}

        public bool TryUpdateCache(string configSet, string environment)
        {
            using (TracerFactory.StartTracer(this, "TryUpdateCache"))
            {
                var config = Repository.GetConfigSet(configSet);
                bool doSave;
                var raw = config.GetRawConfigData(environment,out doSave);
                var environmentSettings = GetEnvironmentSettings(config, environment);
                if(doSave)
                    Repository.UpdateEnvironment(environmentSettings);
                if (realtimeNotificationService != null) realtimeNotificationService(configSet, environment);
                var wrapper = Runtime.CreateRuntimeTask<ICacheManagementWrapper>(environmentSettings.CacheType.CacheType);
                return wrapper.UpdateCache(environmentSettings, raw); 
            }
        }

        public ICacheManagementService Initialize(IConfigSetTask repository)
        {
            Repository = repository;
            return this;
        }

        public void RegisterRealtimeNotificationService(Action<string, string> action)
        {
            realtimeNotificationService = action;
        }

        private IEnvironment GetEnvironmentSettings(IConfigSet config, string environment)
        {
            return (from e in config.Environments where e.Name == environment select e).Single();
        }
    }
}