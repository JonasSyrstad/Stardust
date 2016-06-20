using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Xml;
using Stardust.Core.Service.Web;
using Stardust.Nucleus;
using Stardust.Particles;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Business.CahceManagement;
using Stardust.Starterkit.Configuration.Web.Notification;
using Claim = System.IdentityModel.Claims.Claim;
using ClaimTypes = System.IdentityModel.Claims.ClaimTypes;
using Utilities = Stardust.Interstellar.Utilities.Utilities;

namespace Stardust.Starterkit.Configuration.Web
{

    public class MvcApplication : HttpApplication
    {
        private static IHubContext hub;

        private static HubConnection hubConnection;

        private static IHubProxy hubClient;

        protected void Application_Start()
        {
            this.LoadBindingConfiguration().LoadMapDefinitions<MapDefinitions>();
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_EndRequest(object sender, EventArgs e)
        {
            //var scope = RequestResponseScopefactory.CreateScope();
            var httpApp = (HttpApplication)sender;
            Logging.DebugMessage("EndRequest;");
            Logging.DebugMessage("Current user: {0}", httpApp?.Context?.User?.Identity?.Name);
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

            var httpApp = (HttpApplication)sender;
            Logging.DebugMessage("Authentication request: {0}", httpApp.Request?.Url.ToString());
            if (httpApp.Context != null && httpApp.Context.User != null)
            {
                if (httpApp.Context.User.Identity.IsAuthenticated)
                {
                    Logging.DebugMessage("Do auth....");

                }
            }
            Logging.DebugMessage("Headers: {0}", string.Join(";", httpApp.Request.Headers));
            var authHeader = httpApp.Request.Headers["Authorization"];
            if (authHeader != null)
            {
                var credentials = AuthenticationHeaderValue.Parse(authHeader);
                Logging.DebugMessage("auth schema: {0}", credentials.Scheme);
                if (credentials.Scheme.Equals("bearer", StringComparison.OrdinalIgnoreCase))
                {
                    ValidateToken(credentials.Parameter, httpApp);
                }
            }
        }


        public static void ValidateToken(string accessToken, HttpApplication httpApp)
        {
            var jwt = new JwtSecurityToken(accessToken);
            var handler = new JwtSecurityTokenHandler();
            handler.Configuration = new SecurityTokenHandlerConfiguration
            { CertificateValidationMode = X509CertificateValidationMode.None };

            var validationParameters = new TokenValidationParameters()
            {
                NameClaimType = ClaimTypes.Email,
                ValidAudience = ConfigurationManagerHelper.GetValueOnKey("stardust.fererationAudience"),
                ValidAudiences = new List<string>
                                     {
                                         ConfigurationManagerHelper.GetValueOnKey("stardust.fererationAudience"),
                                         ConfigurationManagerHelper.GetValueOnKey("stardust.fererationAudience") +"/"
                                     },
                ValidIssuer = ConfigurationManagerHelper.GetValueOnKey("stardust.fererationIssuer"),
                IssuerSigningTokens = GetSigningCertificates(ConfigurationManagerHelper.GetValueOnKey("stardust.fererationMetadata"))
            };
            try
            {
                SecurityToken validatedToken;
                Logging.DebugMessage("Validate");
                var securityToken = handler.ValidateToken(accessToken, validationParameters, out validatedToken);
                Logging.DebugMessage("Validated");
                var principal = new ClaimsPrincipal(securityToken);
                var identity = principal.Identity as ClaimsIdentity;
                Thread.CurrentPrincipal = principal;
                HttpContext.Current.User = principal;
                httpApp.Context.User = principal;
                Logging.DebugMessage("UsderClaims");
                foreach (var claim in securityToken.Claims)
                {
                    Logging.DebugMessage("{0} : {1}", claim.Type, claim.Value);
                }


                Logging.DebugMessage("AuthenticatedUser? {0}", identity?.IsAuthenticated);
                Logging.DebugMessage("Current app user: {0}", httpApp?.Context?.User?.Identity?.Name);
            }
            catch (Exception ex)
            {
                ex.Log("Token validation failed");
                throw new UnauthorizedAccessException("Unable to validate token", ex);
            }
            Logging.DebugMessage("Exit token validation....");
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

        protected void Application_Error()
        {
            var ec = Server.GetLastError();
            ec.Log();
        }
    }


}
