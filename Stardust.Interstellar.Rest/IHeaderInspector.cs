using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Stardust.Interstellar.Rest
{
    public interface IHeaderInspector
    {

        IHeaderHandler[] GetHandlers();
    }

    [AttributeUsage(AttributeTargets.Interface|AttributeTargets.Method , AllowMultiple = true)]
    public abstract class HeaderInspectorAttributeBase : Attribute, IHeaderInspector
    {
        public abstract IHeaderHandler[] GetHandlers();
    }

    public interface IAuthenticationInspector
    {
        IAuthenticationHandler GetHandler();
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public abstract class AuthenticationInspectorAttributeBase : Attribute, IAuthenticationInspector
    {
        public abstract IAuthenticationHandler GetHandler();
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class StardustConfigAuthenticationAttribute: AuthenticationInspectorAttributeBase
    {
        public override IAuthenticationHandler GetHandler()
        {
            return new StardustConfigAuthentication();
        }
    }

    
    internal sealed class StardustConfigAuthentication : IAuthenticationHandler
    {
        public void Apply(HttpWebRequest req)
        {
            var token=ConfigurationManager.AppSettings["stardust.accessToken"];
            var tokenKey = ConfigurationManager.AppSettings["stardust.accessTokenKey"];
            var useToken = ConfigurationManager.AppSettings["stardust.useAccessToken"];
            var user = ConfigurationManager.AppSettings["stardust.configUser"];
            var password = ConfigurationManager.AppSettings["stardust.configPassword"];
            var domain = ConfigurationManager.AppSettings["stardust.configDomain"];
            if (useToken != "false")
            {
                req.Headers.Add("key",tokenKey);
                req.Headers.Add("Authorization" ,"Token " + Convert.ToBase64String(Encoding.UTF8.GetBytes(token)));
            }
            else
            {
                req.Credentials=new NetworkCredential(user,password,domain);
            }
        }
    }
}
