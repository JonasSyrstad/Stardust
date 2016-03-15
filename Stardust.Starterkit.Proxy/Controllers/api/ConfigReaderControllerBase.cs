using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Security;
using Stardust.Clusters;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;
using Stardust.Starterkit.Proxy.Models;

namespace Stardust.Starterkit.Proxy.Controllers.api
{
    public class ConfigReaderControllerBase : ApiController
    {

        protected void ValidateAccess(ConfigurationSet configData, string environment)
        {
            if (Request.Headers.Authorization == null) throw new UnauthorizedAccessException("No credentials provided");
            if (Request.Headers.Authorization.Scheme.Equals("token", StringComparison.OrdinalIgnoreCase))
            {
                ValidateToken(configData, environment);
                var userName = string.Format("{0}-{1}", configData.SetName, environment);
                User = new CustomPrincipal(new CustomIdentity(userName, "Anonymous"));
                FormsAuthentication.SetAuthCookie(userName, false);
                return;
            }
            ValidateUsernamePassword(configData);
        }

        private void ValidateUsernamePassword(ConfigurationSet configData)
        {
            var cred = EncodingFactory.ReadFileText(Convert.FromBase64String(Request.Headers.Authorization.Parameter));
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

        public void ValidateToken(ConfigurationSet configData, string environment)
        {
            var token = EncodingFactory.ReadFileText(Convert.FromBase64String(Request.Headers.Authorization.Parameter));
            IEnumerable<string> keys;
            Request.Headers.TryGetValues("key", out keys);
            var keyName = keys == null ? null : keys.FirstOrDefault();
            if (keyName.IsNullOrWhiteSpace()) keyName = configData.SetName + "-" + environment;
            configData.ValidateToken(environment, token, keyName);
        }



        protected HttpResponseMessage CreateUnauthenticatedMessage(UnauthorizedAccessException ex)
        {
            var resp = Request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex);
            resp.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Basic", "Realm=" + Request.RequestUri.AbsolutePath));
            return resp;
        }
    }
}