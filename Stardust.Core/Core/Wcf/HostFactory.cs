//
// HostFactory.cs
// This file is part of Stardust
//
// Author: Jonas Syrstad (jsyrstad2+StardustCore@gmail.com), http://no.linkedin.com/in/jonassyrstad/) 
// Copyright (c) 2014 Jonas Syrstad. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Stardust.Interstellar;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Utilities;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Core.Wcf
{
    /// <summary>
    /// Creates and configures new service instances.
    /// </summary>
    public class HostFactory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            var serviceHost = new ServiceHost(serviceType, baseAddresses);
            serviceHost.Description.Behaviors.Add(Resolver.Activate<IServiceBehavior>("IOC"));
            var configurator = Resolver.Activate<IServiceConfiguration>();
            configurator.ConfigureServiceHost(serviceHost, serviceType);
            SetBehaviour(serviceHost);
            if (!Interstellar.Utilities.Utilities.IsTestEnv() && !Interstellar.Utilities.Utilities.IsDevelopementEnv()) return serviceHost;
            var smb = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (smb == null)
            {
                smb = new ServiceMetadataBehavior();
                serviceHost.Description.Behaviors.Add(smb);
            }
            smb.HttpGetEnabled = true;
            smb.HttpsGetEnabled = true;
            //serviceHost.Description.Behaviors.Add(new ServiceSecurityAuditBehavior{AuditLogLocation = AuditLogLocation.Application});
            serviceHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexHttpBinding(), "mex");
            serviceHost.Description.Behaviors.Add(new ErrorServiceBehavior());
            serviceHost.Faulted += serviceHost_Faulted;
            return serviceHost;
        }

        void serviceHost_Faulted(object sender, EventArgs e)
        {
            Logging.DebugMessage("Service host entered faulted state....", EventLogEntryType.Error);

        }


        private static void SetBehaviour(ServiceHost serviceHost)
        {
            try
            {
                var behaviourSetup = Resolver.Activate<IServiceHostBehaviour>();
                behaviourSetup.ConfigureBehaviours(serviceHost);
                behaviourSetup.SetThrottling(serviceHost);
            }
            catch (Exception ex)
            {
                Logging.Exception(ex, "Failed to set behaviours");
            }
        }
    }

    public class ErrorHandler : IErrorHandler
    {
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (error is FaultException<ErrorMessage>) return;
            var runtime = RuntimeFactory.Current;
            var tracer = runtime.GetTracer();
            FaultException<ErrorMessage> faultException;
            if (tracer != null)
            {
                faultException = new FaultException<ErrorMessage>(new ErrorMessage
                                                                      {
                                                                          Message = error.Message,
                                                                          FaultLocation = tracer.GetCallstack().ErrorPath,
                                                                          TicketNumber = runtime.InstanceId,
                                                                          Detail = ErrorDetail.GetDetails(error)
                                                                      }, error.Message);
            }
            else
            {
                faultException = new FaultException<ErrorMessage>(new ErrorMessage
                {
                    Message = error.Message,
                    Detail = ErrorDetail.GetDetails(error)
                }, error.Message);
            }
            var messageFault = faultException.CreateMessageFault();
            fault = Message.CreateMessage(version, messageFault, "http://stardustframework.com/messaging/fault");

        }

        public bool HandleError(Exception error)
        {
            error.Log();
            return false;
        }
    }

    public class ErrorServiceBehavior : IServiceBehavior
    {
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {

        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            var handler = new ErrorHandler();
            foreach (var channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                try
                {
                    var dispatcher = (ChannelDispatcher)channelDispatcherBase;
                    dispatcher.ErrorHandlers.Add(handler);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }

}
