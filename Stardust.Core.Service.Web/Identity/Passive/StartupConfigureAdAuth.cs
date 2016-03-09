using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Owin.Security.Providers.AzureAD;
using Stardust.Core.Security;
using Stardust.Interstellar;
using Stardust.Particles;

namespace Stardust.Core.Service.Web.Identity.Passive
{
    public static class StartupConfigureAdAuth
    {
        public static IAppBuilder UseStardustAzureAd(this IAppBuilder app, Func<Microsoft.Owin.IOwinContext, Task> handler=null,string tokenEncryptionKey=null)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions { });
            var identitySettings = RuntimeFactory.Current.Context.GetServiceConfiguration().IdentitySettings;
            //app.UseActiveDirectoryFederationServicesBearerAuthentication(
            //    new ActiveDirectoryFederationServicesBearerAuthenticationOptions
            //    {
            //        AuthenticationType = "Bearer",
            //        AuthenticationMode = AuthenticationMode.Active,
            //        Audience = identitySettings.Realm,
            //        MetadataEndpoint =
            //            string.Format("https://{0}/federationmetadata/2007-06/federationmetadata.xml", identitySettings.MetadataUrl),
            //        Realm = identitySettings.Realm,

            //    });
            var options = new AzureADAuthenticationOptions
            {
                ClientId =RuntimeFactory.Current.Context.GetServiceConfiguration().GetConfigParameter("OauthClientId"), //"ab5c2892-b0fe-4530-81e5-9009eb9c8954",
                ClientSecret = RuntimeFactory.Current.Context.GetServiceConfiguration().GetSecureConfigParameter("OauthClientSecret"),//"3yCY/UksdbR/ZdQ/SJPBJDEuay4WOYZXm8R5+88+2bE=",
                AuthenticationMode = AuthenticationMode.Active,
                
                Provider = new AzureADAuthenticationProvider
                {
                    OnAuthenticated = context =>
                    {
                        var token = context.RefreshToken;
                        
                        context.Identity.BootstrapContext = context.RefreshToken;
                        //context.Identity.AddClaim(new Claim("token", token.Encrypt(new EncryptionKeyContainer(Secret(tokenEncryptionKey)))));
                        context.Response.Cookies.Append("sdt", token.Encrypt(new EncryptionKeyContainer(Secret(tokenEncryptionKey))));
                        Logging.DebugMessage(token);
                        return Task.FromResult(0);
                    }
                },

            };
            app.UseAzureADAuthentication(options);
            app.Use(
                async (context, next) =>
                {
                    if (context.Request.User.Identity.IsAuthenticated)
                    {
                        Logging.DebugMessage("Signedin");
                        var token = context.Request.Cookies.Where(c => c.Key == "sdt").Select(c => c.Value).FirstOrDefault();
                        Logging.DebugMessage(token);
                        RuntimeFactory.Current.GetStateStorageContainer().TryAddStorageItem(token ,"oauthToken");
                    }
                    else
                    {
                        Logging.DebugMessage("unauthenticated");
                        context.Authentication.SignOut(context.Authentication.GetAuthenticationTypes().Select(t=>t.AuthenticationType).ToArray());
                        context.Authentication.Challenge();
                    }
                    if (handler != null) await handler(context);
                    await next();
                });
            return app;
        }

        private static string Secret(string tokenEncryptionKey)
        {
            if (tokenEncryptionKey.ContainsCharacters()) ConfigurationManagerHelper.SetValueOnKey("stardust.TokenEncryptionKey",tokenEncryptionKey);
            return tokenEncryptionKey ?? "theeDefaultEncryptionKey";
        }
    }
}
