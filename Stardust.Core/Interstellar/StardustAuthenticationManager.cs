using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Threading;

namespace Stardust.Interstellar
{
    public class StardustAuthenticationManager : ServiceAuthenticationManager
    {
        public static ServiceAuthenticationManager Create()
        {
            return new StardustAuthenticationManager();
        }

        /// <summary>
        /// Authenticates the specified message.
        /// </summary>
        /// <returns>
        /// Returns <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/>.
        /// </returns>
        /// <param name="authPolicy">The authorization policy.</param><param name="listenUri">The URI at which the message was received.</param><param name="message">The message to be authenticated.</param>
        public override ReadOnlyCollection<IAuthorizationPolicy> Authenticate(ReadOnlyCollection<IAuthorizationPolicy> authPolicy, Uri listenUri, ref Message message)
        {
            if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Headers["fedAuth"] != null)
            {

                var tokenString = WebOperationContext.Current.IncomingRequest.Headers["fedAuth"];
                var handler = new Saml2SecurityTokenHandler();
                var token = handler.ReadToken(tokenString);
                var identities = handler.ValidateToken(token);
                var principal = new ClaimsPrincipal(identities);
                Thread.CurrentPrincipal = principal;
            }
            return base.Authenticate(authPolicy, listenUri, ref message);
        }
    }
}