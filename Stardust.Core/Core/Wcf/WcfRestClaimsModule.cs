using System;
using System.Collections.Concurrent;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Xml;
using Stardust.Clusters;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Utilities;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Core.Wcf
{
    public class WcfRestClaimsModule : IHttpModule
    {
        private const string Handling = "handling";

        private static string Securerest { get { return RuntimeFactory.Current.Context.GetConfigParameter("securerest", "securerest"); } }

        private static ConcurrentDictionary<string, string> serviceNameCache = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// You will need to configure this module in the Web.config file of your
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpModule Members

        public void Dispose()
        {
            //clean-up code here.

        }

        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += context_AuthenticateRequest;
            context.EndRequest += context_EndRequest;
        }

        public static void context_EndRequest(object sender, EventArgs e)
        {
            HttpApplication context = sender as HttpApplication;
            if (!IsService(sender, context)) return;
            if (!context.Request.Url.ToString().EndsWith("/securerest") && !HttpContext.Current.Request.Url.ToString().Contains("/securerest/")) return;
            context.Response.AppendHeader("X-Handler", "Stardust 2.1.1");
            if (context.Response.StatusCode != (int)HttpStatusCode.Forbidden)
            {
                return;
            }
            //var realm = string.Format(CultureInfo.InvariantCulture, "BASIC Realm=\"{0}\"", context.Request.Url.AbsolutePath);
            //context.Response.AppendHeader("WWW-Authenticate", realm);
        }

        private static bool IsService(object sender, HttpApplication context)
        {

            if (context != null && context.Context.Request.PhysicalPath.ContainsCharacters())
            {
                var filename = Path.GetFileName(context.Context.Request.PhysicalPath);
                if (filename.IsNullOrWhiteSpace())
                {
                    return false;
                }
                if (!filename.EndsWith("svc", StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        public static void context_AuthenticateRequest(object sender, EventArgs e)
        {

            var context = sender as HttpApplication;
            var auth = context.Request.Headers["Authorization"];
            var credentials = auth.ContainsCharacters()?AuthenticationHeaderValue.Parse(auth):null;
            if (context.Request.Headers["Authorization"] != null && credentials.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) && context.Request.Url.ToString().EndsWith(string.Format("/{0}", RestLoginPage)))
            {
                UseLoginPage(context);
                return;
            }
            if (!IsService(sender, context)) return;
            if (context.Context.Items.Contains(Handling)) return;
            context.Context.Items.Add(Handling, "true");
            if (context.Context != null && context.Context.User != null && context.Context.User.Identity != null && context.Context.User.Identity.IsAuthenticated) return;
            if (Thread.CurrentPrincipal.Identity.IsAuthenticated) return;
            if (!context.Request.Url.ToString().EndsWith(string.Format("/{0}", Securerest)) && !HttpContext.Current.Request.Url.ToString().Contains(string.Format("/{0}/", Securerest))) return;

            try
            {
                if (context.Request.Headers["Authorization"] != null)
                {
                    LogIn(context);
                }
                else
                {
                    ValidateCookie(sender);
                }
            }
            catch (Exception ex)
            {

                ex.Log();
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                HttpContext.Current.Response.End();
            }
        }

        private static void UseLoginPage(HttpApplication context)
        {
            try
            {
                LogIn(context);
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.NoContent;
                HttpContext.Current.Response.End();
            }
            catch (Exception ex)
            {
                ex.Log();
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                HttpContext.Current.Response.End();
            }
            return;
        }

        public static string RestLoginPage
        {
            get
            {
                return RuntimeFactory.Current.Context.GetConfigParameter("stardust.restLogin", "rest.login");
            }
        }

        public static bool DisableBasicOn
        {
            get
            {
                return RuntimeFactory.Current.Context.GetConfigParameter("stardust.disableBasicAuthOnService", "true") == "true";
            }
        }

        private static void ValidateCookie(object sender)
        {
            var context = sender as HttpApplication;
            try
            {
                var handlers = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers;

                var token = FederatedAuthentication.FederationConfiguration.CookieHandler.Read(context.Context);
                var sessionToken = FederatedAuthentication.SessionAuthenticationModule.ReadSessionTokenFromCookie(token);
                var securityToken = handlers.ValidateToken(sessionToken);
                var principal = new ClaimsPrincipal(securityToken);
                Thread.CurrentPrincipal = principal;
                HttpContext.Current.User = principal;
            }
            catch (Exception)
            {
                context.Response.AppendHeader("X-InvalidCredentials", "SessionCookie");
                throw;
            }
        }

        private static void LogIn(HttpApplication context)
        {
            var handlers = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers;
            var auth = context.Request.Headers["Authorization"];
            var credentials = AuthenticationHeaderValue.Parse(auth);
            if (credentials.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) && !DisableBasicOn)
            {
                LoginWithUsernamePassword(credentials, handlers, context);
            }
            else if (credentials.Scheme.Equals("token", StringComparison.OrdinalIgnoreCase))
            {
                LoginWithToken(credentials, handlers, context);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 401;
                HttpContext.Current.Response.End();
            }
        }

        private static void LoginWithToken(AuthenticationHeaderValue credentials, SecurityTokenHandlerCollection handlers, HttpApplication context)
        {
            try
            {
                var token = Convert.FromBase64String(credentials.Parameter);
                using (var stream = new MemoryStream(token))
                {
                    using (var xmlReader = XmlReader.Create(stream))
                    {
                        var securityToken = handlers.ReadToken(xmlReader);
                        var identities = handlers.ValidateToken(securityToken);
                        var principal = new ClaimsPrincipal(identities);
                        var identity = principal.Identity as ClaimsIdentity;
                        if (identity != null) identity.BootstrapContext = new BootstrapContext(token);
                        Thread.CurrentPrincipal = principal;
                        context.Context.User = principal;
                    }
                }
            }
            catch (Exception)
            {
                context.Response.AppendHeader("X-InvalidCredentials", "token");
                throw;
            }
        }

        private static void LoginWithUsernamePassword(AuthenticationHeaderValue credentials, SecurityTokenHandlerCollection handlers, HttpApplication context)
        {
            try
            {
                var cred = EncodingFactory.ReadFileText(Convert.FromBase64String(credentials.Parameter));
                var separator = cred.IndexOf(':');
                var name = cred.Substring(0, separator);
                var password = cred.Substring(separator + 1);
                var manager = new TokenManager(GetServiceName(context), name, password);
                SecurityToken token = null;
                var xmlSecurityToken = manager.GetToken(HttpContext.Current.Request.Url.ToString()) as GenericXmlSecurityToken;
                if (xmlSecurityToken != null)
                {
                    token = handlers.ReadToken(new XmlTextReader(new StringReader(xmlSecurityToken.TokenXml.OuterXml)));
                }
                var securityToken = handlers.ValidateToken(token);
                var principal = new ClaimsPrincipal(securityToken);
                var identity = principal.Identity as ClaimsIdentity;
                if (identity != null) identity.BootstrapContext = new BootstrapContext(xmlSecurityToken.TokenXml.OuterXml);
                Thread.CurrentPrincipal = principal;
                context.Context.User = principal;
                var sessionToken = new SessionSecurityToken(principal);
                FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(sessionToken);
            }
            catch (Exception)
            {
                context.Response.AppendHeader("X-InvalidCredentials", "basic");
                throw;
            }
        }

        private static string GetServiceName(HttpApplication context)
        {
            var url = context.Request.Url.ToString();
            var i = url.IndexOf(Securerest);
            url = url.Remove(i);
            url += Securerest;
            var urlFormater = Resolver.Activate<IUrlFormater>();
            string serviceName = null;
            if (serviceNameCache.TryGetValue(url, out serviceName)) return serviceName;
            foreach (var service in RuntimeFactory.Current.Context.GetRawConfigData<ConfigurationSet>().Endpoints)
            {
                if (service.Endpoints.Any(endpoint => url.Contains(endpoint.Address)))
                {
                    serviceName = service.ServiceName;
                }
                if (serviceName.ContainsCharacters()) break;
            }
            serviceNameCache.TryAdd(url, serviceName);
            return serviceName;
        }

        #endregion

    }
}
