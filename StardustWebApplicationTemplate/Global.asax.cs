using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
using Stardust.Core.Service.Web;
using StardustWebApplicationTemplate.App_Start;

namespace StardustWebApplicationTemplate
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            //Binds and configures the stardust framework
            this.LoadBindingConfiguration<WebBlueprint>()
                .LoadMapDefinitions<MapConfiguration>()
                .MakeClaimsAware()
                .MinifyCommonCookieNames();
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);            
        }
    }
}