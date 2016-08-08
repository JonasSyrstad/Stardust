using Microsoft.AspNet.Identity;
using Owin;
using Stardust.Starterkit.Configuration.Web.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;
using GbSamples.OwinWinAuth;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin.Security.Providers.AzureAD;
using Stardust.Particles;

namespace Stardust.Starterkit.Configuration.Web
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {

            if (ConfigurationManagerHelper.GetValueOnKey<bool>("stardust.useAzureAd"))
            {

                app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

                app.UseCookieAuthentication(new CookieAuthenticationOptions());

                app.UseOpenIdConnectAuthentication(
                    new OpenIdConnectAuthenticationOptions
                    {
                        ClientId = ConfigurationManagerHelper.GetValueOnKey("stardust.fererationClientId"),
                        Authority = ConfigurationManagerHelper.GetValueOnKey("stardust.fererationAuthority"),
                        Notifications = new OpenIdConnectAuthenticationNotifications
                        {
                            AuthenticationFailed = context =>
                            {
                                context.HandleResponse();
                                context.Response.Redirect("/Error?message=" + context.Exception.Message);
                                return Task.FromResult(0);
                            }
                        }
                    });

                app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                    new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                        {
                            Tenant = ConfigurationManagerHelper.GetValueOnKey("stardust.fererationTenant"),
                            TokenValidationParameters =
                                new TokenValidationParameters
                                    {
                                        ValidAudience = ConfigurationManagerHelper.GetValueOnKey("stardust.fererationAudience"),
                                        ValidAudiences =
                                            new List<string>
                                                {
                                                    ConfigurationManagerHelper.GetValueOnKey("stardust.fererationAudience"),
                                                    ConfigurationManagerHelper.GetValueOnKey("stardust.fererationAudience") + "/"
                                                },
                                    }
                        });

            }
        }
    }
}