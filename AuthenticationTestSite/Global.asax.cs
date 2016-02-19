using System.ServiceModel.Web;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AuthenticationTestSite.App_Start;
using Stardust.Core.Default.Implementations.Notification;

namespace AuthenticationTestSite
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            #region This is called from an initialization module that is dynamically added to the application during startup.
            //this.LoadBindingConfiguration<TestBlueprint>()
            //    .MakeClaimsAware()
            //    .MinifyCommonCookieNames(); 
            this.RegisterNotificationHandler();
            #endregion
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
