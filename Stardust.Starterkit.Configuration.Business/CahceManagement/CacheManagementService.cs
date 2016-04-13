using System;
using System.Linq;
using Stardust.Interstellar;
using Stardust.Interstellar.Legacy;
using Stardust.Interstellar.Trace;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business.CahceManagement
{
    public class CacheManagementService : AbstractRuntimeTask, ICacheManagementService
    {
        private readonly IConfigSetTask configSetTask;

        private readonly IEnvironmentTasks environmentTask;

        private static Action<string, string> realtimeNotificationService;

        public CacheManagementService(IRuntime runtime, IConfigSetTask configSetTask, IEnvironmentTasks environmentTask)
            : base(runtime)
        {
            this.configSetTask = configSetTask;
            this.environmentTask = environmentTask;
        }

        public bool TryUpdateCache(string configSet, string environment)
        {
            using (TracerFactory.StartTracer(this, "TryUpdateCache"))
            {
                var config = configSetTask.GetConfigSet(configSet);
                bool doSave;
                var raw = config.GetRawConfigData(environment,out doSave);
                var environmentSettings = GetEnvironmentSettings(config, environment);
                if(doSave)
                    environmentTask.UpdateEnvironment(environmentSettings);
                if (realtimeNotificationService != null) realtimeNotificationService(configSet, environment);
                var wrapper = Runtime.CreateRuntimeTask<ICacheManagementWrapper>(environmentSettings.CacheType.CacheType);
                return wrapper.UpdateCache(environmentSettings, raw); 
            }
        }

        public void RegisterRealtimeNotificationService(Action<string, string> action)
        {
            realtimeNotificationService = action;
        }

        public void NotifyUserChange(string id)
        {
            realtimeNotificationService("user", id);
        }

        private IEnvironment GetEnvironmentSettings(IConfigSet config, string environment)
        {
            return (from e in config.Environments where e.Name == environment select e).Single();
        }
    }
}