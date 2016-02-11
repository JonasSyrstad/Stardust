using System;
using System.Collections.Concurrent;
using System.Linq;
using Newtonsoft.Json;
using StackExchange.Redis;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business.CahceManagement
{
    public class AzureRedisFabricCacheManager : AbstractRuntimeTask, ICacheManagementWrapper
    {
        private static readonly object triowing = new object();
        private const string connectionStringFormat = "{0},ssl={1},password={2}";
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> Multiplexers = new ConcurrentDictionary<string, ConnectionMultiplexer>();
        
        public AzureRedisFabricCacheManager(IRuntime runtime)
            : base(runtime)
        {

        }

        private static string GetConnectionString(Interstellar.ConfigurationReader.CacheSettings caceSettings)
        {
            return string.Format(connectionStringFormat, caceSettings.MachineNames, caceSettings.Secure.ToString().ToLower(), caceSettings.PassPhrase);
        }

        private static string GetItemKey(string setName, string environment)
        {
            return string.Format("CS:{0}_{1}", setName, environment);
        }

        public bool UpdateCache(IEnvironment environmentSettings, ConfigurationSet raw)
        {
            try
            {
                var itemKey = GetItemKey(environmentSettings.ConfigSet.Id, environmentSettings.Name);
                Logging.DebugMessage("Itemkey: {0}",itemKey);
                if(!GetDataStore(raw, environmentSettings.Name).StringSet(itemKey, JsonConvert.SerializeObject(raw),new TimeSpan(1,0,0,0),When.Always, CommandFlags.HighPriority))
                    Logging.DebugMessage("Update cache item {0} failed",itemKey );
                GetMultiplexer(raw, environmentSettings.Name)
                    .GetSubscriber()
                    .PublishAsync(itemKey, JsonConvert.SerializeObject(raw)).Wait();
            }
            catch (System.Exception ex)
            {
                Logging.Exception(ex, "[REDIS]");
                return false;
            }
            return true;
        }

        private static IDatabase GetDataStore(ConfigurationSet item, string environment)
        {

            return GetMultiplexer(item, environment).GetDatabase();
        }

        private static ConnectionMultiplexer GetMultiplexer(ConfigurationSet item, string environment)
        {
            var itemKey = GetItemKey(item.SetName, environment);
            var env = item.Environments.Single(e => e.EnvironmentName == environment);
            ConnectionMultiplexer multiplexer;
            if (Multiplexers.TryGetValue(itemKey, out multiplexer)) return multiplexer;
            lock (triowing)
            {
                if (Multiplexers.TryGetValue(itemKey, out multiplexer)) return multiplexer;
                multiplexer = ConnectionMultiplexer.Connect(GetConnectionString(env.Cache));
                Multiplexers.TryAdd(itemKey, multiplexer);
                return multiplexer;
            }
        }
    }

}