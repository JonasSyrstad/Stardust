using System.Web;

namespace Stardust.Core.Security
{
    public interface IWebServerConfiguration
    {
        void PrepWebServer(HttpApplication host);
    }
}