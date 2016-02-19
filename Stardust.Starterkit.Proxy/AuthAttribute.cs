using System.Security.Principal;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Stardust.Particles;

namespace Stardust.Starterkit.Proxy
{
    public class AuthAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Determines whether client is authorized to connect to <see cref="T:Microsoft.AspNet.SignalR.Hubs.IHub"/>.
        /// </summary>
        /// <param name="hubDescriptor">Description of the hub client is attempting to connect to.</param><param name="request">The (re)connect request from the client.</param>
        /// <returns>
        /// true if the caller is authorized to connect to the hub; otherwise, false.
        /// </returns>
        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            Logging.DebugMessage("AuthorizeHubConnection");
            return true;
        }

        /// <summary>
        /// Determines whether client is authorized to invoke the <see cref="T:Microsoft.AspNet.SignalR.Hubs.IHub"/> method.
        /// </summary>
        /// <param name="hubIncomingInvokerContext">An <see cref="T:Microsoft.AspNet.SignalR.Hubs.IHubIncomingInvokerContext"/> providing details regarding the <see cref="T:Microsoft.AspNet.SignalR.Hubs.IHub"/> method invocation.</param><param name="appliesToMethod">Indicates whether the interface instance is an attribute applied directly to a method.</param>
        /// <returns>
        /// true if the caller is authorized to invoke the <see cref="T:Microsoft.AspNet.SignalR.Hubs.IHub"/> method; otherwise, false.
        /// </returns>
        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            Logging.DebugMessage("AuthorizeHubMethodInvocation");
            return true;
        }

        /// <summary>
        /// When overridden, provides an entry point for custom authorization checks.
        ///             Called by <see cref="M:Microsoft.AspNet.SignalR.AuthorizeAttribute.AuthorizeHubConnection(Microsoft.AspNet.SignalR.Hubs.HubDescriptor,Microsoft.AspNet.SignalR.IRequest)"/> and <see cref="M:Microsoft.AspNet.SignalR.AuthorizeAttribute.AuthorizeHubMethodInvocation(Microsoft.AspNet.SignalR.Hubs.IHubIncomingInvokerContext,System.Boolean)"/>.
        /// </summary>
        /// <param name="user">The <see cref="T:System.Security.Principal.IPrincipal"/> for the client being authorize</param>
        /// <returns>
        /// true if the user is authorized, otherwise, false
        /// </returns>
        protected override bool UserAuthorized(IPrincipal user)
        {
            Logging.DebugMessage("UserAuthorized");
            return true;
        }
    }
}