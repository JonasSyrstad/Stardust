using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using Stardust.Core.Service.Web;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;
using Stardust.Starterkit.Proxy.App_Start;
using Stardust.Starterkit.Proxy.Controllers;
using Stardust.Starterkit.Proxy.Models;
using GlobalConfiguration = System.Web.Http.GlobalConfiguration;

namespace Stardust.Starterkit.Proxy
{
    public class Global : HttpApplication
    {
        private static HubConnection hubConnection;

        private static IHubProxy hub;

        private static IHubContext hubSender;

        void Application_Start(object sender, EventArgs e)
        {
            this.LoadBindingConfiguration<ProxyBlupeprint>();
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.UseRealtimeUpdate") == "true")
                RegisterForNotifications();

            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        private static void RegisterForNotifications()
        {
            try
            {
                GlobalHost.HubPipeline.RequireAuthentication();
                hubSender = GlobalHost.ConnectionManager.GetHubContext<ConfigSetHub>();
                hubConnection = new HubConnection(Utilities.GetConfigLocation());
                hub = hubConnection.CreateHubProxy("configSetHub");
                hub.On("changed",
                    (string id, string environment) =>
                    {
                        Logging.DebugMessage("Config set updated: {0}-{1}",id,environment);
                        var fileName = ConfigCacheHelper.GetLocalFileName(id, environment);
                        ConfigCacheHelper.GetConfiguration(id, environment, fileName);
                        hubSender.Clients.Group(string.Format("{0}-{1}", id, environment))
                            .notify(ConfigCacheHelper.GetConfiguration(id, environment, fileName));
                    });
                hubConnection.Start();
                hubConnection.EnsureReconnecting();
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var error = Server.GetLastError();
            var message = error.Message;
        }


    }
    [HubName("configSetHub")]
    public class ConfigSetHub : Hub
    {
        [HubMethodName("changed")]
        public void ConfigSetUpdated(string id, string environment)
        {
            Clients.All.changed(id, environment);
        }

        /// <summary>
        /// Called when the connection connects to this hub instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task"/>
        /// </returns>
        public override Task OnConnected()
        {
            var set = this.Context.Headers["set"];
            var env = this.Context.Headers["env"];
            var token = this.Context.Headers["Token"];
            var configSet = ConfigCacheHelper.GetConfigFromCache(ConfigCacheHelper.GetLocalFileName(set, env));
            if(configSet==null) throw new UnauthorizedAccessException("Unknown config");
            if (configSet.Set.AllowMasterKeyAccess && configSet.Set.ReaderKey == token)
                return base.OnConnected();
            if (configSet.Set.Environments.SingleOrDefault(e => e.EnvironmentName == env).ReaderKey == token) return OnConnected();
            throw new UnauthorizedAccessException("Invalid token");

        }
    }
}