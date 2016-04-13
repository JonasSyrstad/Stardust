using System;
using System.IdentityModel.Services;
using System.Web;
using System.Web.Security;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Stardust.Core.Wcf;
using Stardust.Particles;

namespace Stardust.Core.Security
{
    class WebServerConfiguration : IWebServerConfiguration
    {
        public static bool IsConfiguredAsWebFront { get; private set; }
            
        public void PrepWebServer(HttpApplication host)
        {
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.IsBackendServer")=="false")
            {
                DynamicModuleUtility.RegisterModule(typeof (SessionAuthenticationModule));
                DynamicModuleUtility.RegisterModule(typeof (WSFederationAuthenticationModule));
                IsConfiguredAsWebFront = true;
            }
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.SecureWcfRest") == "true")
                DynamicModuleUtility.RegisterModule(typeof(WcfRestClaimsModule));
            var modules = ConfigurationManagerHelper.GetValueOnKey("stardust.registerModules");
            if (modules.ContainsCharacters())
            {
                foreach (var moduleTypeName in modules.Split('|'))
                {
                    DynamicModuleUtility.RegisterModule(Type.GetType(moduleTypeName));
                }
            }
        }
    }
}