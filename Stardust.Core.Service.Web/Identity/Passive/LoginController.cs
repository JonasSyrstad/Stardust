using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Stardust.Interstellar;
using Stardust.Particles;

namespace Stardust.Core.Service.Web.Identity.Passive
{
    
    public abstract class AuthController:BaseController
    {
        protected AuthController(IRuntime runtime)
            : base(runtime)
        {
        }

        public virtual ActionResult Login(string returnUrl)
        {
            return new ChallengeResult(OpenIdConnectAuthenticationDefaults.AuthenticationType, Url.Action("Callback"));
        }

        public virtual ActionResult Logout(string returnUrl)
        {
            HttpContext.GetOwinContext().Authentication.SignOut(HttpContext.GetOwinContext().Authentication.GetAuthenticationTypes().Select(t => t.AuthenticationType).ToArray());
            return RedirectToAction(returnUrl);
        }

        public virtual ActionResult Callback(string returnUrl)
        {
            HttpContext.GetOwinContext().Authentication.SignIn(new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            }, (ClaimsIdentity)HttpContext.GetOwinContext().Authentication.User.Identity);
            if (returnUrl.IsNullOrWhiteSpace()) returnUrl = "/";
            return Redirect(returnUrl);
        }
    }

    public class ChallengeResult : HttpUnauthorizedResult
    {
        public ChallengeResult(string provider, string redirectUri)
            : this(provider, redirectUri, null)
        { }

        public ChallengeResult(string provider, string redirectUri, string userId)
        {
            LoginProvider = provider;
            RedirectUri = redirectUri;
            UserId = userId;
        }

        public string LoginProvider { get; set; }
        public string RedirectUri { get; set; }
        public string UserId { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
            if (UserId != null)
                properties.Dictionary[XsrfKey] = UserId;

            var owin = context.HttpContext.GetOwinContext();
            owin.Authentication.Challenge(properties, LoginProvider);
        }
        private const string XsrfKey = "CodePaste_$31!.2*#";
    }
}
