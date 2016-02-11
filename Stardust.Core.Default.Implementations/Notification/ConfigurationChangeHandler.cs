using System;
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
    class StarterkitConfigurationReaderEx : StarterkitConfigurationReader
    {
        private static Action<ConfigurationSet> handler;

        /// <summary>
        /// This will only be used if the Reader implementation supports change notification. not implemented in this, but if you change the cache by overriding this remember to hook up this.
        /// </summary>
        /// <param name="onCacheChanged"/>
        public override void SetCacheInvalidatedHandler(Action<ConfigurationSet> onCacheChanged)
        {
            handler = onCacheChanged;

        }

        internal static void Notify(ConfigurationSet configSet)
        {
            if (handler != null) handler(configSet);
        }
    }

    public class ConfigurationChangeHandler
    {
        private static HubConnection hubConnection;

        private static IHubProxy hub;

        public static void RegisterNotificationHandler(IAppBuilder app)
        {
            hubConnection = new HubConnection(Utilities.GetConfigLocation()) { GroupsToken = string.Format("{0}-{1}", GetConfigSetName(), GetEnvironmentName()) };
            hub = hubConnection.CreateHubProxy("configSetHub");
            hub.On(
                "notify",
                (ConfigurationSet configSet) =>
                    {
                        MemoryCache.Default.Remove(string.Format("{0}{1}", GetConfigSetName(), GetEnvironmentName()));
                        StarterkitConfigurationReaderEx.Notify(configSet);
                    });
            hubConnection.EnsureReconnecting();
            hubConnection.ConnectionToken=
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
