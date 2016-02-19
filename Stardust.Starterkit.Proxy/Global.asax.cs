using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.SignalR.Client;
using Stardust.Core.Service.Web;
using Stardust.Starterkit.Proxy.App_Start;
using GlobalConfiguration = System.Web.Http.GlobalConfiguration;

namespace Stardust.Starterkit.Proxy
{
    public class Global : HttpApplication
    {
        private static HubConnection hubConnection;

        private static IHubProxy hub;

        void Application_Start(object sender, EventArgs e)
        {
            this.LoadBindingConfiguration<ProxyBlupeprint>();
                

            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

       

        protected void Application_Error(object sender, EventArgs e)
        {
            var error = Server.GetLastError();
            var message = error.Message;
        }


    }
}