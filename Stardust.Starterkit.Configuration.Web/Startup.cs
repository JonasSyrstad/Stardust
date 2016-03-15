using System;
using System.Net;
using GbSamples.OwinWinAuth;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Owin;
using Owin;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Business.CahceManagement;
using Stardust.Starterkit.Configuration.Web.Notification;
using Utilities = Stardust.Interstellar.Utilities.Utilities;

[assembly: OwinStartupAttribute(typeof(Stardust.Starterkit.Configuration.Web.Startup))]
namespace Stardust.Starterkit.Configuration.Web
{
    public partial class Startup
    {
        private static IHubContext hub;

        private static HubConnection hubConnection;

        private static IHubProxy hubClient;

       

        public void Configuration(IAppBuilder app)
        {
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.UseAzureAd") == "true")
            {
                ConfigureAuth(app);
            }
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.UseRealtimeUpdate") == "true")
            {
               
                app.MapSignalR("/signalr", new HubConfiguration
                                               {
                                                   EnableJSONP = true,
                                                   EnableDetailedErrors = true,
                                                   EnableJavaScriptProxies = true
                                               });
                hub = GlobalHost.ConnectionManager.GetHubContext<ConfigSetHub>();
                Resolver.Activate<ICacheManagementService>().RegisterRealtimeNotificationService(
                    (id, environment) =>
                        {
                            try
                            {
                                Logging.DebugMessage("Sending update message {0}-{0}",id,environment);
                                hub.Clients.All.changed(id, environment);
                            }
                            catch (Exception ex)
                            {
                                ex.Log();
                            }
                        });
                if(!Utilities.IsDevelopementEnv()) return;
                hubConnection = new HubConnection("https://localhost:44305/");
                hubClient = hubConnection.CreateHubProxy("configSetHub");
                hubClient.On(
                    "changed",
                    (string id, string environment) =>
                        {
                            Logging.DebugMessage("UpdateMessage: {0}-{1}", id, environment);
                        });
                hubConnection.Start();
            }
        }
    }
}
