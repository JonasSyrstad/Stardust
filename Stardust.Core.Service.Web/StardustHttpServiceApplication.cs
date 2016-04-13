using System.Security.Claims;
using System.Web;
using Stardust.Interstellar;
using Stardust.Particles;

namespace Stardust.Core.Service.Web
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StardustHttpServiceApplication<T> : HttpApplication where T : Blueprint, new()
    {
        protected ISetupContext configContext;

        public virtual void Application_Start()
        {
            configContext = this.LoadBindingConfiguration<T>();
            ConfigureAuth();
        }

        /// <summary>
        /// Sets the application to use saml passive federation, override this to do custom setup
        /// </summary>
        protected virtual void ConfigureAuth()
        {
            configContext.MakeClaimsAware().MinifyCommonCookieNames().SetAntiForgeryToken(ClaimTypes.NameIdentifier);
        }

        protected virtual bool UseRealtimeNotifications
        {
            get
            {
                return ConfigurationManagerHelper.GetValueOnKey("stardust.useRealtimeNotifications") != "false";
            }
        }
    }
}