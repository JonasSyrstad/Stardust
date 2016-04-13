using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using System.Net.Http;
using Microsoft.Owin.Security;

namespace Stardust.Core.Service.Web.Identity.Passive
{
    public class OAuthAuthenticationCallbackHandler : IHttpModule
    {
        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application </param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += context_BeginRequest;
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            var path = HttpContext.Current.Request.Url.AbsolutePath;
            if (!HttpContext.Current.Request.IsAuthenticated && string.Equals(path, "/oauth.login", StringComparison.OrdinalIgnoreCase))
            {
                HttpContext.Current.GetOwinContext().Authentication.Challenge(new AuthenticationProperties
                                                                                          {
                                                                                              AllowRefresh = true,
                                                                                              IsPersistent = true,
                                                                                              
                                                                                          });
                HttpContext.Current.Response.End();
            }
            else if(string.Equals(HttpContext.Current.Request.Url.AbsolutePath, "oauth.authorize", StringComparison.OrdinalIgnoreCase))
            {
                var authenticateResult = Task.Run(async ()=>await HttpContext.Current.GetOwinContext().Authentication.AuthenticateAsync("ExternalCookie")).Result;
                
            }
        }



        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
            
        }
    }
}
