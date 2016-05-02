using System;
using System.Configuration;
using System.Net;
using System.Text;

namespace Stardust.Interstellar.Rest
{
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