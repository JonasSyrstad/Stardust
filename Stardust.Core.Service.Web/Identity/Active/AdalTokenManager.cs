using System;
using System.IdentityModel.Claims;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Stardust.Core.Security;
using Stardust.Core.Service.Web.Identity.Passive;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;

namespace Stardust.Core.Service.Web.Identity.Active
{
    public class AdalTokenManager
    {
        /// <summary>
        /// Call this to force adal to prompt user for credentials. Primarily used by powershell and other native clients.
        /// </summary>
        /// <param name="prompt"></param>
        public static void PromptUserForCredentials(bool prompt)
        {
            ConfigurationManagerHelper.SetValueOnKey("stardust.promptUserFOrCredentials", prompt.ToString().ToLower(), true);
        }

        /// <summary>
        /// Call this to force adal to prompt user for credentials. Primarily used by powershell and other native clients.
        /// </summary>
        public static void PromptUserForCredentials()
        {
            PromptUserForCredentials(true);
        }

        public static AuthenticationResult GetToken(string serviceName)
        {
            var ctx = new AuthenticationContext(IdentitySettings.IssuerAddress, new NativeTokenCache());
            var resource = Resource(serviceName);
            var appClientId = RuntimeFactory.Current.Context.GetServiceConfiguration().GetConfigParameter("OauthClientId");
            var appClientSecret = RuntimeFactory.Current.Context.GetServiceConfiguration().GetSecureConfigParameter("OauthClientSecret");
            try
            {
                AuthenticationResult token;
                var userToken = GetUserToken();
                var userId = GetUserObjectId();
                var clientCredential = new ClientCredential(appClientId, appClientSecret);
                if (userToken.ContainsCharacters()) token = ctx.AcquireToken(resource, clientCredential, new UserAssertion(userToken));
                else if (userId.ContainsCharacters()) token = ctx.AcquireTokenSilent(resource, clientCredential, GetUserAssertion());
                else
                {
                    if (ConfigurationManagerHelper.GetValueOnKey("stardust.promptUserFOrCredentials", false)) token = ctx.AcquireToken(resource, appClientId, new Uri("http://" + Utilities.GetEnvironment() + "ters.dnvgl.com"),PromptBehavior.Auto);
                    else token=ctx.AcquireToken(resource, clientCredential);
                }
                return token;
            }
            catch (AdalSilentTokenAcquisitionException adalex)
            {
                if (adalex.ErrorCode == AdalError.FailedToAcquireTokenSilently)
                {
                    HttpContext.Current.GetOwinContext().Authentication.SignOut();
                    HttpContext.Current.GetOwinContext().Authentication.Challenge();
                    throw;
                }
                throw;
            }
            catch (System.Exception ex)
            {
                ex.Log();
                throw;
            }

        }

        private static UserIdentifier GetUserAssertion()
        {
            var userObjectID = GetUserObjectId();
            return new UserIdentifier(userObjectID, UserIdentifierType.UniqueId);
        }

        private static string GetUserObjectId()
        {
            var user = RuntimeFactory.Current.GetCurrentClaimsIdentity();
            var userObjectID = user?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            return userObjectID;
        }

        private static string GetUserToken()
        {
            try
            {
                var user = RuntimeFactory.Current.GetCurrentClaimsIdentity();
                var userObjectID = user?.FindFirst("token");
                if (userObjectID == null) return null;
                return userObjectID.Value;
            }
            catch
            {
                return null;
            }
        }

        private static string CurrentUser()
        {
            var claim = RuntimeFactory.Current.GetCurrentClaimsIdentity().FindFirst(s => s.Type == ClaimTypes.NameIdentifier);
            return claim.Value;
        }

        public static string AdalDelegateToken
        {
            get
            {
                var container = RuntimeFactory.Current.GetStateStorageContainer();
                string token;
                if (container.TryGetItem("oauthToken", out token)) return token.Decrypt(new EncryptionKeyContainer(Secret()));
                return null;
            }
        }

        private static string Secret()
        {
            return ConfigurationManagerHelper.GetValueOnKey("stardust.TokenEncryptionKey").ContainsCharacters() ? ConfigurationManagerHelper.GetValueOnKey("stardust.TokenEncryptionKey") : "theeDefaultEncryptionKey";
        }

        public static string ServiceAccountName
        {
            get
            {
                var _ServiceAccountName = RuntimeFactory.Current.Context.GetEnvironmentConfiguration().GetConfigParameter("ServiceAccountName");
                if (string.IsNullOrEmpty(_ServiceAccountName))
                {
                    return string.Empty;
                }
                else
                {
                    return _ServiceAccountName;
                }
            }
        }

        public static string ServiceAccountPassword
        {
            get
            {
                var _ServiceAccountPassword = RuntimeFactory.Current.Context.GetEnvironmentConfiguration().GetSecureConfigParameter("ServiceAccountPassword");
                if (string.IsNullOrEmpty(_ServiceAccountPassword))
                {
                    return string.Empty;
                }
                else
                {
                    return _ServiceAccountPassword;
                }
            }
        }
        private static IdentitySettings IdentitySettings
        {
            get
            {
                return RuntimeFactory.Current.Context.GetServiceConfiguration().IdentitySettings;
            }
        }

        private static string Resource(string serviceName)
        {
            return RuntimeFactory.Current.Context.GetEndpointConfiguration(serviceName).GetEndpoint(RuntimeFactory.Current.Context.GetEndpointConfiguration(serviceName).ActiveEndpoint).Audience;
        }
    }
}