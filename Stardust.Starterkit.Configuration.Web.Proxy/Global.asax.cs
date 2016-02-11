using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using Hangfire;
using Hangfire.MemoryStorage;
using Newtonsoft.Json;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Web.Proxy.Controllers;

namespace Stardust.Starterkit.Configuration.Web.Proxy
{
    public static class ConfigCacheHelper
    {
        private static ConcurrentDictionary<string, ConfigWrapper> cache = new ConcurrentDictionary<string, ConfigWrapper>();

        public static ConfigWrapper GetConfigFromCache(string localFile)
        {
            ConfigWrapper config;
            if (!cache.TryGetValue(localFile, out config))
            {
                config = JsonConvert.DeserializeObject<ConfigWrapper>(File.ReadAllText(localFile));
                cache.TryAdd(localFile, config);
            }
            return config;
        }

        public static void UpdateCache(string configSet, ConfigurationSet newConfigSet, ConfigWrapper cs)
        {
            var config = new ConfigWrapper { Set = newConfigSet, Environment = cs.Environment, Id = cs.Id };
            ConfigWrapper oldConfig;
            if (!cache.TryGetValue(configSet, out oldConfig)) cache.TryAdd(configSet, config);
            else
            {
                cache.TryUpdate(configSet, config, oldConfig);
            }
            File.WriteAllText(configSet, JsonConvert.SerializeObject(config));
        }
    }
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            //GlobalConfiguration.Configuration.UseStorage(new MemoryStorage());
            //Updates the files every 10 min
            //RecurringJob.AddOrUpdate(() => UpdateConfigSets(), "0/10 * * * *");
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            var error = Server.GetLastError();
            var message = error.Message;
        }

        public void UpdateConfigSets()
        {
            var configSets = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\App_Data");
            foreach (var configSet in configSets)
            {
                var cs = JsonConvert.DeserializeObject<ConfigWrapper>(File.ReadAllText(configSet));
                var newConfigSet = ConfigReaderController.GetConfiguration(cs.Id, cs.Environment, configSet, true);
                if (newConfigSet.LastUpdated > cs.Set.LastUpdated) ConfigCacheHelper.UpdateCache(configSet, newConfigSet, cs);
            }
        }
    }

}
