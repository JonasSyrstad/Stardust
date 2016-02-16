using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Owin;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Nucleus;
using Stardust.Particles;
using Utilities = Stardust.Interstellar.Utilities.Utilities;

namespace Stardust.Core.Default.Implementations.Notification
{
    public class ConfigurationChangeHandler
    {
        private static HubConnection hubConnection;

        private static IHubProxy hub;

        public static void RegisterNotificationHandler(IAppBuilder app)
        {
            hubConnection = new HubConnection(Utilities.GetConfigLocation()) { GroupsToken = string.Format("{0}-{1}", GetConfigSetName(), GetEnvironmentName()) };
            hub = hubConnection.CreateHubProxy("configSetHub");
            hub.On("notify",
                (ConfigurationSet configSet) =>
                    {
                        MemoryCache.Default.Remove(string.Format("{0}{1}", GetConfigSetName(), GetEnvironmentName()));
                        StarterkitConfigurationReaderEx.Notify(configSet);
                    });
            hubConnection.Headers.Add("set",GetConfigSetName());
            hubConnection.Headers.Add("env",GetEnvironmentName());
            hubConnection.Headers.Add("Token", ConfigurationManagerHelper.GetValueOnKey("stardust.accessToken"));
            hubConnection.EnsureReconnecting();
            hubConnection.Start();
        }

        private static string GetConfigSetName()
        {
            return ConfigurationManagerHelper.GetValueOnKey("configSet");
        }

        private static string GetEnvironmentName()
        {
            return ConfigurationManagerHelper.GetValueOnKey("environment");
        }
    }
}
