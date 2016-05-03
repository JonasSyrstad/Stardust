using System;
using System.IO;
using System.Net;
using System.Timers;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Owin;
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

        internal static HubConnection hubConnection;

        private static IHubProxy hub;

        private static Timer timer;

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
                var userName = GetConfigServiceUser();
                var password = GetConfigServicePassword();
                GlobalHost.HubPipeline.RequireAuthentication();
                hubConnection = new HubConnection(GetConfigLocation());
                if (hubConnection.CookieContainer == null) hubConnection.CookieContainer = new CookieContainer();
                //hubConnection.Credentials = new NetworkCredential(userName, password, GetConfigServiceDomain());
                ConfigCacheHelper.SetCredentials(hubConnection);
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
                hubConnection.StateChanged += hubConnection_StateChanged;
                hubConnection.Start();
                CreatePingService();
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        private static void CreatePingService()
        {
            timer = new Timer(10000);
            timer.Start();
            timer.Elapsed += Timer_Elapsed;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            hub.Invoke("ping", Environment.MachineName);
        }

        static void hubConnection_StateChanged(StateChange obj)
        {
            Logging.DebugMessage("Going from {0} to {1} on {2}", obj.OldState, obj.NewState, GetConfigLocation());
        }

        static void hubConnection_Error(Exception obj)
        {
            obj.Log("Signalr connection issue");
            if (obj is UnauthorizedAccessException || obj.InnerException is UnauthorizedAccessException)
            {
                Logging.DebugMessage("Token expiry... refreshing oauth token");
                try
                {
                    hubConnection.Stop();
                }
                catch (Exception)
                {
                    // ignored
                }
                ConfigCacheHelper.SetCredentials(hubConnection);
                hubConnection.Start();
            }
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
            var location = Utilities.GetConfigLocation();
            if (!location.StartsWith("http")) location = "https://" + location;
            return location;
        }

        private static void UpdateUser(string name)
        {
            try
            {
                Logging.DebugMessage("Updating user {0}", name);
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
