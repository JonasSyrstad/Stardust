using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using Owin.Security.Providers.AzureAD;
using Stardust.Core.Security;
using Stardust.Interstellar;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;

namespace Stardust.Core.Service.Web.Identity.Passive
{
    public static class StartupConfigureAdAuth
    {
        public const string ResourceKey = "resourceid";
        public static IAppBuilder UseStardustAzureAd(this IAppBuilder app, Func<Microsoft.Owin.IOwinContext, Task> handler = null, string tokenEncryptionKey = null)
        {
            app.SetDefaultSignInAsAuthenticationType(DefaultAuthenticationTypes.ExternalCookie);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ExternalCookie,
                AuthenticationMode = AuthenticationMode.Active,
                
                CookieName = ".sd.ec",
                ExpireTimeSpan = TimeSpan.FromMinutes(5),
                CookieManager = new SystemWebCookieManager(),
                SlidingExpiration = true,
                CookieSecure = CookieSecureOption.Always,
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = context =>
                        {
                                context.ReplaceIdentity(context.Identity);
                            return Task.FromResult(0);
                        },
                    OnResponseSignIn = context =>
                        {
                            Logging.DebugMessage(context.AuthenticationType);
                        }
                },
                LoginPath = PathString.FromUriComponent("/auth/login"),
                LogoutPath = PathString.FromUriComponent("/auth/logout"),


            });
            var identitySettings = RuntimeFactory.Current.Context.GetServiceConfiguration().IdentitySettings;

            var options = new AzureADAuthenticationOptions
            {
                ClientId = ClientId, //"ab5c2892-b0fe-4530-81e5-9009eb9c8954",
                ClientSecret = ClientSecret,//"3yCY/UksdbR/ZdQ/SJPBJDEuay4WOYZXm8R5+88+2bE=",
                AuthenticationMode = AuthenticationMode.Passive,
                Provider = new AzureADAuthenticationProvider
                               {
                                   OnAuthenticated = context =>
                                       {
                                           context.Identity.AddClaim(new Claim("AccessToken", context.AccessToken));
                                           context.Identity.AddClaim(new Claim("RefreshToken", context.RefreshToken));
                                           return Task.FromResult(0);
                                       },

                               },
                Resource = { identitySettings.Realm },

            };
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                                                   {
                                                       AuthenticationMode = AuthenticationMode.Passive,
                                                       SignInAsAuthenticationType = DefaultAuthenticationTypes.ExternalCookie,
                                                       ClientId = ClientId,
                                                       Authority = identitySettings.IssuerAddress.StartsWith("https://") ? identitySettings.IssuerAddress : "https://" + identitySettings.IssuerAddress,
                                                       //MetadataAddress = identitySettings.MetadataUrl.StartsWith("https://") ? identitySettings.MetadataUrl : "https://" + identitySettings.MetadataUrl ,
                                                       Notifications = new OpenIdConnectAuthenticationNotifications
                                                                           {
                                                                               AuthorizationCodeReceived = context =>
                                                                                   {
                                                                                       if (context.OwinContext.Request.User.Identity.IsAuthenticated)
                                                                                       {
                                                                                           return Task.FromResult(0);
                                                                                       }
                                                                                       var code = context.Code;
                                                                                       var credential = new ClientCredential(ClientId, ClientSecret);
                                                                                       var authContext = new AuthenticationContext(identitySettings.IssuerAddress, new NativeTokenCache());
                                                                                       var result = authContext.AcquireTokenByAuthorizationCode(code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, "https://graph.windows.net");
                                                                                       var principal = new ClaimsPrincipal(context.AuthenticationTicket.Identity);
                                                                                       Thread.CurrentPrincipal = principal;
                                                                                       HttpContext.Current.User = principal;
                                                                                       context.OwinContext.Authentication.SignIn((ClaimsIdentity)principal.Identity);
                                                                                       return Task.FromResult(0);
                                                                                   },
                                                                               RedirectToIdentityProvider = (context) =>
                                                                               {
                                                                                   if (context.OwinContext.Authentication.AuthenticationResponseChallenge != null)
                                                                                   {
                                                                                       if (context.OwinContext.Authentication.AuthenticationResponseChallenge.Properties.Dictionary.ContainsKey(ResourceKey))
                                                                                       {
                                                                                           context.ProtocolMessage.Resource = context.OwinContext.Authentication.AuthenticationResponseChallenge.Properties.Dictionary[ResourceKey];
                                                                                       }
                                                                                   }
                                                                                   return Task.FromResult(0);
                                                                               }
                                                                           }
                                                   });
            app.UseAzureADAuthentication(options);
            new SetupContext(null).MakeOAuthAwareService();
            return app;
        }

        private static string ClientSecret
        {
            get
            {
                return RuntimeFactory.Current.Context.GetServiceConfiguration().GetSecureConfigParameter("OauthClientSecret");
            }
        }

        private static string ClientId
        {
            get
            {
                return RuntimeFactory.Current.Context.GetServiceConfiguration().GetConfigParameter("OauthClientId");
            }
        }

        private static string Secret(string tokenEncryptionKey)
        {
            if (tokenEncryptionKey.ContainsCharacters()) ConfigurationManagerHelper.SetValueOnKey("stardust.ConfigKey", tokenEncryptionKey);
            return tokenEncryptionKey ?? "theeDefaultEncryptionKey";
        }
    }
}
