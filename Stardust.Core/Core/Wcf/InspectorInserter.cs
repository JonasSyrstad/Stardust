using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Stardust.Interstellar.Endpoints;

namespace Stardust.Core.Wcf
{
    public class InspectorInserter : BehaviorExtensionElement, IServiceBehavior, IEndpointBehavior, IOperationBehavior
    {
        #region IServiceBehavior Members
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        { }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }

        #endregion
        #region IEndpointBehavior Members

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            var inspector=new ClientHeaderInspector();
            clientRuntime.ClientMessageInspectors.Add(inspector);
            foreach (var op in clientRuntime.Operations)
                op.ParameterInspectors.Add(inspector);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            var inspector = new ServiceHeaderInspector();
            if (endpoint.Binding is SecureWebHttpBinding)
                endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new RestSecurityHeaderInspector());
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
            foreach (var op in endpointDispatcher.DispatchRuntime.Operations)
            {
                op.ParameterInspectors.Add(inspector);
            }
        }

        public void Validate(ServiceEndpoint endpoint) { }
        #endregion
        #region IOperationBehavior Members
        public void AddBindingParameters(
          OperationDescription operationDescription, BindingParameterCollection bindingParameters
        )
        { }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
        }

        public void Validate(OperationDescription operationDescription) { }

        #endregion

        public override Type BehaviorType
        {
            get { return typeof(InspectorInserter); }
        }

        protected override object CreateBehavior()
        { return new InspectorInserter(); }
    }


}
