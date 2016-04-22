using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR.Client;
using Owin;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Nucleus;
using Stardust.Particles;
using Utilities = Stardust.Interstellar.Utilities.Utilities;

namespace Stardust.Core.Default.Implementations.Notification
{
    public static class ConfigurationChangeHandler
    {
        private static HubConnection hubConnection;

        private static IHubProxy hub;

        // <summary>
        /// Registers the application for configuration change notification events.
        /// </summary>
        /// <param name="app"></param>
        public static void RegisterNotificationHandler(this IAppBuilder app)
        {
            RegisterUpdateHandler();
        }

        /// <summary>
        /// Registers the application for configuration change notification events.
        /// </summary>
        /// <param name="app"></param>
        public static void RegisterNotificationHandler(this HttpApplication app)
        {
            RegisterUpdateHandler();
        }

        private static void RegisterUpdateHandler()
        {
            //ContainerFactory.Current.InvalidateBinding(typeof(IConfigurationReader), Scope.Singleton);
            //Resolver.GetConfigurator().UnBind<IConfigurationReader>().AllAndBind().To<StarterkitConfigurationReaderEx>().SetSingletonScope();
            hubConnection = new HubConnection(Utilities.GetConfigLocation())
                                {

                                    //GroupsToken = string.Format("{0}-{1}", GetConfigSetName(), GetEnvironmentName())
                                };
            hub = hubConnection.CreateHubProxy("notificationHub");
            hub.On("notify",
                (string id, string env) =>
                {
                    if (id != GetConfigSetName() || env != GetEnvironmentName())
                    {
                        Logging.DebugMessage("NotMe, exiting");
                        return;
                    }
                    Logging.DebugMessage("update config");
                    MemoryCache.Default.Remove(string.Format("{0}{1}", GetConfigSetName(), GetEnvironmentName()));
                    var reader = Resolver.Activate<IConfigurationReader>();
                    var set=reader.GetConfiguration(id, env);
                    StarterkitConfigurationReaderEx.Notify(set);
                });
            hub.On("joinConfirmation", s => { Logging.DebugMessage("Join successfull: {0}", s); });
            var key = ConfigurationManagerHelper.GetValueOnKey("stardust.accessTokenKey");
            hubConnection.Headers.Add("set", GetConfigSetName());
            hubConnection.Headers.Add("env", GetEnvironmentName());
            var keyName = key.ContainsCharacters() ? key : string.Format("{0}-{1}", GetConfigSetName(), GetEnvironmentName()).ToLower();
            hubConnection.Headers.Add("key", keyName);
            hubConnection.Headers.Add("Token", GetAccessToken());
            hubConnection.CookieContainer = new CookieContainer();
            hubConnection.EnsureReconnecting();
            Logging.DebugMessage("{0} cookies", hubConnection.CookieContainer.Count);
            Task.Run(async () =>
                {
                    await StartNotificationHandler(key, keyName);
                });



        }

        private static async Task StartNotificationHandler(string key, string keyName)
        {
            using (var loginRequest = new CookieAwareWebClient{Container = new CookieContainer()})
            {

                try
                {
                    loginRequest.Headers.Add("env", GetEnvironmentName());
                    loginRequest.Headers.Add("set", GetConfigSetName());
                    loginRequest.Headers.Add("key", keyName);
                    loginRequest.Headers.Add("Authorization", "token " + GetAccessToken());
                    var site = string.Join("/", Utilities.GetConfigLocation(), "Auth");
                    var result =  loginRequest.DownloadString(new Uri(site)); 
                    hubConnection.CookieContainer.Add(loginRequest.Cookies);
                    Logging.DebugMessage("Auth result="+result);
                    Logging.DebugMessage("Starting hub connection."); 
                    Logging.DebugMessage("{0} cookies", hubConnection.CookieContainer.Count); 
                    await hubConnection.Start();
                }
                catch (Exception ex)
                {
                    ex.Log(); 
                    return;
                }

                Logging.DebugMessage("{0} cookies", loginRequest.Container.Count);
            }
            try
            {
                Logging.DebugMessage("Subscribing to notifications...");
                await hub.Invoke("join", GetConfigSetName(), GetEnvironmentName());
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        private static string GetAccessToken()
        {
            return Convert.ToBase64String(ConfigurationManagerHelper.GetValueOnKey("stardust.accessToken").GetByteArray());
        }

        private static string GetConfigSetName()
        {
            return ConfigurationManagerHelper.GetValueOnKey("configSet");
        }

        private static string GetEnvironmentName()
        {
            return Utilities.GetEnvironment();
        }
    }
}
