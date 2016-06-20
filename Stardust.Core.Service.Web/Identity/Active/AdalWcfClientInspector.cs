using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Newtonsoft.Json;
using Stardust.Interstellar;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Serializers;
using Stardust.Nucleus;
using Stardust.Particles;
using Utilities = Stardust.Interstellar.Utilities.Utilities;

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
                request.Properties.Add(HttpRequestMessageProperty.Name, httpRequest);
            }
            if (httpRequest != null)
            {
                httpRequest.Headers.Add("Authorization", string.Format("{0}", AdalTokenManager.GetToken(serviceName).CreateAuthorizationHeader()));
                var supportCode = "";
                RuntimeFactory.Current.GetStateStorageContainer().TryGetItem("supportCode", out supportCode);
                var meta = new RequestHeader
                             {
                                 Environment = Utilities.GetEnvironment(),
                                 ConfigSet = Utilities.GetConfigSetName(),
                                 ServiceName = RuntimeFactory.Current.ServiceName,
                                 MessageId = Guid.NewGuid().ToString(),
                                 ServerIdentity = Environment.MachineName,
                                 RuntimeInstance = RuntimeFactory.Current.InstanceId.ToString(),
                                 SupportCode = supportCode
                             };
                httpRequest.Headers.Add("X-Stardust-Meta", Convert.ToBase64String(JsonConvert.SerializeObject(meta).GetByteArray()));
            }

            return RuntimeFactory.Current;

        }

        /// <summary>
        /// Enables inspection or modification of a message after a reply message is received but prior to passing it back to the client application.
        /// </summary>
        /// <param name="reply">The message to be transformed into types and handed back to the client application.</param><param name="correlationState">Correlation state data.</param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            try
            {
                var runtime = correlationState as IRuntime;
                if (runtime == null) return;
                HttpResponseMessageProperty httpRequest;
                if (reply.Properties.ContainsKey(HttpResponseMessageProperty.Name))
                {
                    httpRequest = reply.Properties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
                    var msg = httpRequest.Headers["X-Stardust-Meta"];
                    if (msg.ContainsCharacters())
                    {
                        try
                        {
                            var item = Resolver.Activate<IReplaceableSerializer>().Deserialize<ResponseHeader>(Convert.FromBase64String(msg).GetStringFromArray());
                            if (runtime.CallStack == null) return;
                            if (runtime.CallStack.CallStack != null) runtime.CallStack.CallStack.Add(item.CallStack);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    //if ((int)httpRequest.StatusCode < 200 || (int)httpRequest.StatusCode > 204)
                    //{
                    //    try
                    //    {
                    //        var errorMsg = reply.GetBody<ErrorMessage>(new DataContractJsonSerializer(typeof(ErrorMessage)));
                    //        throw new FaultException<ErrorMessage>(errorMsg);
                    //    }
                    //    catch (Exception ex)
                    //    {

                    //        throw new StardustCoreException("Unable to grab error body....",ex);
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }

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
}
