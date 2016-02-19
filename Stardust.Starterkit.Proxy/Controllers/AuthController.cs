using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Stardust.Clusters;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;
using Stardust.Starterkit.Proxy.Controllers.api;
using Stardust.Starterkit.Proxy.Models;

namespace Stardust.Starterkit.Proxy.Controllers
{
    public class AuthController : Controller
    {
        // GET: Auth
        public ActionResult Index()
        {
            try
            {
                var env = Request.Headers["env"];
                var set = Request.Headers["set"];
                var localfile = ConfigCacheHelper.GetLocalFileName(set, env);
                var configData = ConfigCacheHelper.GetConfigFromCache(localfile);
                if (configData == null)
                {
                    ConfigCacheHelper.GetConfiguration(set, env, localfile);
                    configData = ConfigCacheHelper.GetConfigFromCache(localfile);
                }
                ValidateAccess(configData.Set, env);
                return new ContentResult { Content = "OK", ContentEncoding = Encoding.UTF8, ContentType = "" };
            }
            catch (Exception ex)
            {
                ex.Log();
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Invalid credentials");
            }

        }

        protected void ValidateAccess(ConfigurationSet configData, string environment)
        {
            var auth = Request.Headers["Authorization"];
            if (auth.IsNullOrWhiteSpace()) throw new UnauthorizedAccessException("No credentials provided");
            var token = auth.Split(' ').Last();
            if (auth.StartsWith("token", StringComparison.OrdinalIgnoreCase))
            {
                ValidateToken(configData, environment, token);
            }
            else
                ValidateUsernamePassword(configData, token);
            var userName = string.Format("{0}-{1}", configData.SetName, environment);
            this.HttpContext.User = new CustomPrincipal(new CustomIdentity(userName, "Form"));
            Thread.CurrentPrincipal = HttpContext.User;
            FormsAuthentication.SetAuthCookie(userName, false);
        }

        private void ValidateUsernamePassword(ConfigurationSet configData, string token)
        {
            var cred = EncodingFactory.ReadFileText(Convert.FromBase64String(token));
            var separator = cred.IndexOf(':');
            var name = cred.Substring(0, separator);
            var password = cred.Substring(separator + 1);
            var nameParts = name.Split('\\');
            if (nameParts.Length == 1)
            {
                if (configData.SetName.Equals(nameParts[0], StringComparison.OrdinalIgnoreCase) && configData.ReaderKey.Decrypt(ConfigCacheHelper.Secret) == password)
                {
                    return;
                }
            }
            else if (nameParts.Length == 2)
            {
                if (configData.SetName.Equals(nameParts[0], StringComparison.OrdinalIgnoreCase))
                {
                    var env = configData.Environments.SingleOrDefault(e => e.EnvironmentName.Equals(nameParts[1], StringComparison.OrdinalIgnoreCase));
                    if (env != null && env.ReaderKey.Decrypt(ConfigCacheHelper.Secret) == password)
                    {

                        return;
                    }
                }
            }
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        public void ValidateToken(ConfigurationSet configData, string environment, string tokenString)
        {
            var token = EncodingFactory.ReadFileText(Convert.FromBase64String(tokenString));
            var keyName = Request.Headers["key"];
            if (keyName.IsNullOrWhiteSpace()) keyName = configData.SetName + "-" + environment;
            configData.ValidateToken(environment, token, keyName);
        }
    }
}