using System;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using Stardust.Particles;
using Stardust.Starterkit.Proxy.Models;

[assembly: OwinStartup(typeof(Stardust.Starterkit.Proxy.App_Start.Startup))]

namespace Stardust.Starterkit.Proxy.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.UseRealtimeUpdate") == "true") return;
            GlobalConfiguration.Configuration.UseStorage(new MemoryStorage());
            app.UseHangfireDashboard();
            app.UseHangfireServer();
            RecurringJob.AddOrUpdate("update", () => UpdateConfigSets(), () => string.Format("*/{0} * * * *", GetInterval()));

        }

        private string GetInterval()
        {
            var i = ConfigurationManagerHelper.GetValueOnKey("stardust.UpdateInterval");
            if (i.IsNullOrWhiteSpace()) return "5";
            return i;
        }

        public static void UpdateConfigSets()
        {
            Logging.DebugMessage("Updating files");
            var configSets = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\App_Data");
            foreach (var configSet in configSets)
            {
                var cs = JsonConvert.DeserializeObject<ConfigWrapper>(File.ReadAllText(configSet));
                var newConfigSet = ConfigCacheHelper.GetConfiguration(cs.Id, cs.Environment, configSet, true);
                ConfigCacheHelper.UpdateCache(configSet, newConfigSet, cs);
            }
        }
    }
}
