using System;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Xml;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    internal class RestTokenHandler : IEndpointBehavior, IClientMessageInspector
    {
        private readonly string servicename;

        public RestTokenHandler(string servicename)
        {
            this.servicename = servicename;
        }

        public TokenManager TokenManager { get; set; }

        /// <summary>
        /// Implement to confirm that the endpoint meets some intended criteria.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(ServiceEndpoint endpoint)
        {

        }

        /// <summary>
        /// Implement to pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="endpoint">The endpoint to modify.</param><param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {

        }

        /// <summary>
        /// Implements a modification or extension of the service across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that exposes the contract.</param><param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {

        }

        /// <summary>
        /// Implements a modification or extension of the client across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param><param name="clientRuntime">The client runtime to be customized.</param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(new RestTokenHandler(servicename));
        }

        /// <summary>
        /// Enables inspection or modification of a message before a request message is sent to a service.
        /// </summary>
        /// <returns>
        /// The object that is returned as the <paramref name="correlationState "/>argument of the <see cref="M:System.ServiceModel.Dispatcher.IClientMessageInspector.AfterReceiveReply(System.ServiceModel.Channels.Message@,System.Object)"/> method. This is null if no correlation state is used.The best practice is to make this a <see cref="T:System.Guid"/> to ensure that no two <paramref name="correlationState"/> objects are the same.
        /// </returns>
        /// <param name="request">The message to be sent to the service.</param><param name="channel">The WCF client object channel.</param>
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var runtime = RuntimeFactory.Current;
            var httpHeader = request.Properties["httpRequest"] as HttpRequestMessageProperty;
            var manager = runtime.GetStateStorageContainer().GetItem<TokenManager>();
            SecurityToken rawToken;
            if (runtime.GetBootstrapContext() == null) rawToken = manager.GetToken(channel.RemoteAddress.ToString());
            else rawToken = manager.GetTokenOnBehalfOf(channel.RemoteAddress.ToString()) ;
            SecurityToken token = null;
            var xmlSecurityToken = rawToken as GenericXmlSecurityToken;
            ;
            string tokenString;
            using (var xmlreader = new StringReader(xmlSecurityToken.TokenXml.OuterXml))
            {
                tokenString = xmlreader.ReadToEnd();
            }
            //if (rawToken != null) token = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers.ReadToken(new XmlTextReader());
            httpHeader.Headers.Add("Authorization", string.Format("Token {0}", Convert.ToBase64String(tokenString.GetByteArray())));
            return runtime;
        }



        /// <summary>
        /// Enables inspection or modification of a message after a reply message is received but prior to passing it back to the client application.
        /// </summary>
        /// <param name="reply">The message to be transformed into types and handed back to the client application.</param><param name="correlationState">Correlation state data.</param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            var runtime = correlationState as IRuntime;
            
        }

        public void SetContainer(ServiceContainer<object> serviceContainer)
        {
            
        }
    }
}