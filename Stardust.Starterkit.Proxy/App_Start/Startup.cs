using System;
using System.IO;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Newtonsoft.Json;
using Owin;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;
using Stardust.Starterkit.Proxy.Models;

[assembly: OwinStartup(typeof(Stardust.Starterkit.Proxy.App_Start.Startup))]

namespace Stardust.Starterkit.Proxy.App_Start
{
    public class Startup
    {

        private static HubConnection hubConnection;

        private static IHubProxy hub;

        public void Configuration(IAppBuilder app)
        {
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.UseRealtimeUpdate") == "true")
            {
                app.MapSignalR();
                GlobalHost.HubPipeline.RequireAuthentication();
                RegisterForNotifications();
                return;
            }
            GlobalConfiguration.Configuration.UseStorage(new MemoryStorage());
            app.UseHangfireDashboard();
            app.UseHangfireServer();
            RecurringJob.AddOrUpdate("update", () => UpdateConfigSets(), () => string.Format("*/{0} * * * *", GetInterval()));

        }

        private static void RegisterForNotifications()
        {
            try
            {
                GlobalHost.HubPipeline.RequireAuthentication();
                hubConnection = new HubConnection(Utilities.GetConfigLocation());
                hub = hubConnection.CreateHubProxy("configSetHub");
                hub.On("changed",
                    (string id, string environment) =>
                    {
                        Logging.DebugMessage("Config set updated: {0}-{1}", id, environment);
                        var fileName = ConfigCacheHelper.GetLocalFileName(id, environment);
                        ConfigCacheHelper.GetConfiguration(id, environment, fileName);
                        try
                        {
                            var hubSender = GlobalHost.ConnectionManager.GetHubContext("notificationHub");
                            var groupName = string.Format("{0}-{1}", id, environment).ToLower();
                            hubSender.Clients.Group(groupName).notify(id, environment);
                        }
                        catch (Exception ex)
                        {
                            ex.Log("failure sending message");
                        }
                    });
                hubConnection.EnsureReconnecting();
                hubConnection.Start();

            }
            catch (Exception ex)
            {
                ex.Log();
            }
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
