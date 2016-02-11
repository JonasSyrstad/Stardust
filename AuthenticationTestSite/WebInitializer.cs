using AuthenticationTestSite.App_Start;
using Stardust.Core;
using Stardust.Core.Service.Web;

namespace AuthenticationTestSite
{
    public class WebInitializer : IStardustWebInitializer
    {
        public void Initialize(ConfigWrapper instance)
        {
            instance.AddMvcHooks().MakeClaimsAware().MinifyCommonCookieNames();
           
        }
    }
}