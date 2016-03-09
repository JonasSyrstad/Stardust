using System;
using System.Collections.Concurrent;
using System.IdentityModel.Claims;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Stardust.Core.Security;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;

namespace Stardust.Core.Service.Web.Identity.Active
{
    public class AdalWcfClientInspector : IClientMessageInspector, IEndpointBehavior
    {
        private readonly string serviceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public AdalWcfClientInspector()
        {
        }

        private AdalWcfClientInspector(string serviceName)
        {
            this.serviceName = serviceName;
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
            HttpRequestMessageProperty httpRequest;
            if (request.Properties.ContainsKey(HttpRequestMessageProperty.Name))
            {
                httpRequest = request.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            }
            else
            {
                httpRequest = new HttpRequestMessageProperty();
                request.Properties.Add(HttpRequestMessageProperty.Name,httpRequest);
            }
            if (httpRequest != null)
            {
                httpRequest.Headers.Add("Authorization", string.Format("{0}", AdalTokenManager.GetToken(serviceName).CreateAuthorizationHeader()));
            }

            return null;

        }

        /// <summary>
        /// Enables inspection or modification of a message after a reply message is received but prior to passing it back to the client application.
        /// </summary>
        /// <param name="reply">The message to be transformed into types and handed back to the client application.</param><param name="correlationState">Correlation state data.</param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {

        }

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
            var sn = endpoint.Behaviors.Find<ServiceNameAttribute>();

            clientRuntime.ClientMessageInspectors.Add(new AdalWcfClientInspector(sn == null ? endpoint.Contract.Name : sn.ServiceName));
        }
    }
    internal static class KeyHelper
    {
        internal static EncryptionKeyContainer SharedSecret
        {
            get
            {
            return new EncryptionKeyContainer(GetKeyFromConfig());
            }
        }

        private static string GetKeyFromConfig()
        {
            var key = ConfigurationManagerHelper.GetValueOnKey("stardust.ConfigKey");
            if (key.ContainsCharacters()) return key;
            key = "defaultEncryptionKey";
            ConfigurationManagerHelper.SetValueOnKey("stardust.ConfigKey", key, true);
            return key;
        }
    }


    public class AdalTokenManager
    {
        private static ConcurrentDictionary<string,AuthenticationResult> tokenCache=new ConcurrentDictionary<string, AuthenticationResult>(); 
        public static AuthenticationResult GetToken(string serviceName)
        {
            
            Logging.DebugMessage("Adding bearer token....");
            Logging.DebugMessage(AdalDelegateToken);
            AuthenticationResult token;
            if (tokenCache.TryGetValue(CurrentUser(), out token))
            {
                if (token.ExpiresOn >= DateTimeOffset.UtcNow) return token;
                AuthenticationResult oldToken;
                tokenCache.TryRemove(CurrentUser(), out oldToken);
            }
            var service = RuntimeFactory.Current.Context.GetEndpointConfiguration(serviceName);
            var endpoint = service.GetEndpoint(service.ActiveEndpoint);
            var ctx = new AuthenticationContext(IdentitySettings.MetadataUrl.StartsWith("http") ? IdentitySettings.MetadataUrl : ("https://" + IdentitySettings.MetadataUrl));
            var clientId = endpoint.PropertyBag["ClientId"];
            var resource = Resource(serviceName);
            var appClientId = RuntimeFactory.Current.Context.GetServiceConfiguration().GetConfigParameter("ClientId");
            var appClientSecret = RuntimeFactory.Current.Context.GetServiceConfiguration().GetConfigParameter("ClientSecret").Decrypt(KeyHelper.SharedSecret);
            if (AdalDelegateToken == null) token = ctx.AcquireToken(resource, appClientId, new UserCredential(ServiceAccountName, ServiceAccountPassword));
            else token = ctx.AcquireTokenByRefreshToken(AdalDelegateToken, new ClientCredential(appClientId,appClientSecret)); //AcquireToken(resource, clientId, assertion);
            tokenCache.TryAdd(CurrentUser(), token);
            return token;
            
        }

        private static string CurrentUser()
        {
            var claim= RuntimeFactory.Current.GetCurrentClaimsIdentity().FindFirst(s=>s.Type==ClaimTypes.NameIdentifier);
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
