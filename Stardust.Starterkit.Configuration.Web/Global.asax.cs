﻿using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Stardust.Core.Service.Web;
using Stardust.Nucleus;
using Stardust.Particles;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Stardust.Starterkit.Configuration.Business.CahceManagement;
using Stardust.Starterkit.Configuration.Web.Notification;
using Utilities = Stardust.Interstellar.Utilities.Utilities;

namespace Stardust.Starterkit.Configuration.Web
{

    public class MvcApplication : HttpApplication
    {
        private static IHubContext hub;

        private static HubConnection hubConnection;

        private static IHubProxy hubClient;

        protected void Application_Start()
        {
            this.LoadBindingConfiguration().LoadMapDefinitions<MapDefinitions>();
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.UseRealtimeUpdate") == "true")
                CreateNotificationService();
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private static void CreateNotificationService()
        {

            
        }

        protected void Application_Error()
        {
            var ec = Server.GetLastError();
            ec.Log();
        }
    }


}
