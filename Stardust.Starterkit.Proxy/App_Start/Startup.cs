using System;
using System.IO;
using System.Net;
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
using Stardust.Starterkit.Configuration.Web;
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
        private static string GetConfigServiceDomain()
        {
            return ConfigurationManagerHelper.GetValueOnKey("stardust.configDomain");
        }

        private static string GetConfigServicePassword()
        {
            return ConfigurationManagerHelper.GetValueOnKey("stardust.configPassword");
        }

        private static string GetConfigServiceUser()
        {
            return ConfigurationManagerHelper.GetValueOnKey("stardust.configUser");
        }
        private static void RegisterForNotifications()
        {
            try
            {
                //var client = new CookieAwareWebClient(){Container = new CookieContainer()};
                var userName = GetConfigServiceUser();
                var password = GetConfigServicePassword();
                //if (userName.ContainsCharacters() && password.ContainsCharacters())
                //{
                //    client.Credentials = new NetworkCredential(userName, password, GetConfigServiceDomain());
                //}
                //var result = client.DownloadString(GetConfigLocation() + "/Account/Signin");

                GlobalHost.HubPipeline.RequireAuthentication();
                hubConnection = new HubConnection(GetConfigLocation());
                if(hubConnection.CookieContainer==null) hubConnection.CookieContainer=new CookieContainer();
                hubConnection.Credentials = new NetworkCredential(userName, password, GetConfigServiceDomain());
                hub = hubConnection.CreateHubProxy("configSetHub");
                hub.On("changed",
                    (string id, string name) =>
                    {
                        if (string.Equals(id, "user", StringComparison.OrdinalIgnoreCase))
                        {
                            UpdateUser(name);
                        }
                        else
                        {
                            UpdateEnvironmet(id, name);
                        }
                    });
                hubConnection.EnsureReconnecting();
                hubConnection.Reconnecting += hubConnection_Reconnecting;
                hubConnection.Closed += hubConnection_Closed;
                hubConnection.Error += hubConnection_Error;
                hubConnection.Start();

            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        static void hubConnection_Error(Exception obj)
        {
            obj.Log("Signalr connection issue");
        }

        static void hubConnection_Closed()
        {
            Logging.DebugMessage("Why did it disconnect??");
        }

        static void hubConnection_Reconnecting()
        {
            Logging.DebugMessage("Reconnection signalr");
        }

        private static string GetConfigLocation()
        {
            var location= Utilities.GetConfigLocation();
            if (!location.StartsWith("http")) location = "https://" + location;
            return location;
        }

        private static void UpdateUser(string name)
        {
            try
            {
                UserValidator.UpdateUser(name);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        private static void UpdateEnvironmet(string id, string name)
        {
            Logging.DebugMessage("Config set updated: {0}-{1}", id, name);
            var fileName = ConfigCacheHelper.GetLocalFileName(id, name);
            ConfigCacheHelper.GetConfiguration(id, name, fileName);
            try
            {
                var hubSender = GlobalHost.ConnectionManager.GetHubContext("notificationHub");
                var groupName = string.Format("{0}-{1}", id, name).ToLower();
                hubSender.Clients.Group(groupName).notify(id, name);
            }
            catch (Exception ex)
            {
                ex.Log("failure sending message");
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
