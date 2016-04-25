using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Stardust.Clusters;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;

namespace Stardust.Core.Service.Web.Identity.Active
{
    public class AdalWcfModule : IHttpModule
    {
        private const string Bearer = "bearer";

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application </param>
        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += context_AuthenticateRequest;
            context.EndRequest += context_EndRequest;
        }

        private void context_EndRequest(object sender, EventArgs e)
        {
            HttpApplication context = sender as HttpApplication;
            if (!IsService(sender, context)) return;
            //if (!context.Request.Url.ToString().EndsWith("/securerest") && !HttpContext.Current.Request.Url.ToString().Contains("/securerest/")) return;
            context.Response.AppendHeader("X-Handler", "Stardust 2.1.1");
            if (context.Response.StatusCode != (int)HttpStatusCode.Forbidden)
            {
                return;
            }
        }

        private void context_AuthenticateRequest(object sender, EventArgs e)
        {

            try
            {
                var context = sender as HttpApplication;
                if (!IsService(sender, context)) return;
                var auth = context.Request.Headers["Authorization"];
                var credentials = AuthenticationHeaderValue.Parse(auth);
                if (context.Request.Headers["Authorization"] != null)
                {
                    LogIn(context);
                }
            }
            catch (Exception ex)
            {

                ex.Log();
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                HttpContext.Current.Response.End();
            }

        }

        private static void LogIn(HttpApplication context)
        {
            var auth = context.Request.Headers["Authorization"];
            var credentials = AuthenticationHeaderValue.Parse(auth);
            if (credentials.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) && !DisableAuthOnService)
            {
                LoginWithUsernamePassword(credentials, context);
            }
            else if (credentials.Scheme.Equals(Bearer, StringComparison.OrdinalIgnoreCase))
            {
                LoginWithToken(credentials, context);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 401;
                HttpContext.Current.Response.End();
            }
        }

        public static bool DisableAuthOnService
        {
            get
            {
                return RuntimeFactory.Current.Context.GetConfigParameter("stardust.disableBasicAuthOnService", "true") == "true";

            }
        }

        private static void LoginWithToken(AuthenticationHeaderValue credentials, HttpApplication context)
        {
            var token = credentials.Parameter;
            ValidateToken(token, context);
        }

        private static void LoginWithUsernamePassword(AuthenticationHeaderValue credentials, HttpApplication context)
        {
            var cred = EncodingFactory.ReadFileText(Convert.FromBase64String(credentials.Parameter));
            var separator = cred.IndexOf(':');
            var name = cred.Substring(0, separator);
            var password = cred.Substring(separator + 1);
            var ctx = new AuthenticationContext(RuntimeFactory.Current.Context.GetServiceConfiguration().IdentitySettings.IssuerAddress);
            var token = ctx.AcquireToken(Resource, RuntimeFactory.Current.Context.GetConfigParameter("ServiceAccountName"), new UserCredential(name, password));
            var accessToken = token.AccessToken;
            ValidateToken(accessToken, context);
        }

        private static void ValidateToken(string accessToken, HttpApplication context)
        {
            var jwt = new JwtSecurityToken(accessToken);
            var handler = new JwtSecurityTokenHandler();
            handler.Configuration = new SecurityTokenHandlerConfiguration { CertificateValidationMode = X509CertificateValidationMode.None };
            var audience = RuntimeFactory.Current.Context.GetServiceConfiguration().GetConfigParameter("Address");
            if (!audience.StartsWith("https://")) audience = string.Format("https://{0}/", audience);

            var validationParameters = new TokenValidationParameters()
                                           {
                                               ValidAudience = audience,
                                               ValidIssuer = IdentitySettings.IssuerName,
                                               IssuerSigningTokens = GetSigningCertificates(string.Format("{0}", IdentitySettings.MetadataUrl))
                                           };
            SecurityToken validatedToken;
            var securityToken = handler.ValidateToken(accessToken, validationParameters, out validatedToken);
            ((ClaimsIdentity)securityToken.Identity).AddClaim(new Claim("token", accessToken));
            var principal = new ClaimsPrincipal(securityToken);
            var identity = principal.Identity as ClaimsIdentity;
            Thread.CurrentPrincipal = principal;
            context.Context.User = principal;
            HttpContext.Current.User = principal;
        }
        public static ConcurrentDictionary<string, List<SecurityToken>> cache = new ConcurrentDictionary<string, List<SecurityToken>>();
        public static List<SecurityToken> GetSigningCertificates(string metadataAddress)
        {
            Logging.DebugMessage(metadataAddress);
            List<SecurityToken> tokens;
            if (cache.TryGetValue(metadataAddress, out tokens)) return tokens;
            tokens = new List<SecurityToken>();

            if (metadataAddress == null)
            {
                throw new ArgumentNullException(metadataAddress);
            }

            using (XmlReader metadataReader = XmlReader.Create(metadataAddress))
            {
                MetadataSerializer serializer = new MetadataSerializer()
                {
                    // Do not disable for production code
                    CertificateValidationMode = X509CertificateValidationMode.None
                };

                EntityDescriptor metadata = serializer.ReadMetadata(metadataReader) as EntityDescriptor;

                if (metadata != null)
                {
                    var stsd = metadata.RoleDescriptors.OfType<SecurityTokenServiceDescriptor>().First();

                    if (stsd != null)
                    {

                        var x509DataClauses = stsd.Keys.Where(key => key.KeyInfo != null && (key.Use == KeyType.Signing || key.Use == KeyType.Unspecified)).
                                                             Select(key => key.KeyInfo.OfType<X509RawDataKeyIdentifierClause>().First());
                        tokens.AddRange(x509DataClauses.Select(token => new X509SecurityToken(new X509Certificate2(token.GetX509RawData()))));
                    }
                    else
                    {
                        throw new InvalidOperationException("There is no RoleDescriptor of type SecurityTokenServiceType in the metadata");
                    }
                }
                else
                {
                    throw new Exception("Invalid Federation Metadata document");
                }
            }
            cache.TryAdd(metadataAddress, tokens);
            return tokens;
        }


        private static byte[] RawCertificate()
        {
            return Convert.FromBase64String(IdentitySettings.Certificate);
        }

        private static IdentitySettings IdentitySettings
        {
            get
            {
                return RuntimeFactory.Current.Context.GetServiceConfiguration().IdentitySettings;
            }
        }

        private static string Resource
        {
            get
            {
                return RuntimeFactory.Current.Context.GetServiceConfiguration().IdentitySettings.Realm;
            }
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

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {

        }

    }
}
