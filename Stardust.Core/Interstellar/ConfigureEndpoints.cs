//
// ConfigureEndpoints.cs
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
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using Stardust.Core.Wcf;
using Stardust.Interstellar.FrameworkInitializers;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;
using Binding = System.ServiceModel.Channels.Binding;
using Stardust.Interstellar.ConfigurationReader;

namespace Stardust.Interstellar
{
    public class ConfigureEndpoints : IServiceConfiguration
    {
        private IRuntime Runtime;

        public IRuntime GetRuntime()
        {
            if (Runtime.IsNull())
                Runtime = RuntimeFactory.CreateRuntime();
            return Runtime;
        }

        public void ConfigureServiceHost(ServiceHost selfConfiguringHost, Type serviceType)
        {
            var serviceInterfaces = serviceType.GetInterfaces();
            var serviceName = ServiceHostHelper.GetServiceName(serviceType, serviceInterfaces);
            var serviceRootUrl = GetRuntime().Context.GetEnvironmentConfiguration().GetConfigParameter(ServiceHostHelper.GetServiceRootUrl(GetRuntime(), serviceType, serviceInterfaces));
            var contractDescriptions = new Dictionary<Type, ContractDescription>();

            foreach (var serviceInterfaceEndpoint in GetEndpoints(serviceName))
            {
                if (serviceInterfaceEndpoint.Ignore) continue;
                var address = ServiceHostHelper.EndpointAddress(serviceName, serviceInterfaceEndpoint, serviceRootUrl);
                var binding = CreateBinding(serviceInterfaceEndpoint);
                if (serviceInterfaces.IsNull()) throw new StardustCoreException("Service interface not found");
                foreach (var type in serviceInterfaces)
                {
                    if (ServiceHostHelper.InterfaceIsNotServiceContract(type)) continue;
                    if (!LegalEndpoint(type, serviceInterfaceEndpoint.EndpointName)) continue;
                    AddContractDescription(contractDescriptions, type);
                    var cd = contractDescriptions[type];

                    cd.Namespace = "http://service.dyn.no/" + type.Name;
                    try
                    {
                        var se = CreateEndpoint(binding, address, cd, serviceInterfaceEndpoint);
                        se.Behaviors.Add(new InspectorInserter());
                        if (binding is WebHttpBinding)
                        {
                            if (se.Behaviors.Find<WebHttpBehavior>() == null)
                            {
                                se.Behaviors.Add(
                                    new WebHttpBehavior
                                        {
                                            DefaultOutgoingRequestFormat = WebMessageFormat.Json,
                                            DefaultOutgoingResponseFormat = WebMessageFormat.Json,
                                            AutomaticFormatSelectionEnabled = true,
                                            DefaultBodyStyle = WebMessageBodyStyle.Wrapped
                                        });
                            }
                        }
                        AddEndpoint(selfConfiguringHost, address, se);
                    }
                    catch (Exception ex)
                    {

                        throw new StardustCoreException("Failed to create and add endpoint", ex);
                    }
                }
            }
        }

        private static void AddContractDescription(Dictionary<Type, ContractDescription> contractDescriptions, Type type)
        {
            if (!contractDescriptions.ContainsKey(type))
            {
                try
                {
                    contractDescriptions.Add(type, ContractDescription.GetContract(type));
                }
                catch (Exception ex)
                {
                    throw new StardustCoreException("Failed to add contract description", ex);
                }
            }
        }

        private static Binding CreateBinding(Endpoint serviceInterfaceEndpoint)
        {
            try
            {
                return ServiceHostHelper.CreateBinding(serviceInterfaceEndpoint);
            }
            catch (Exception ex)
            {
                throw new StardustCoreException(string.Format("Failed to create binding for protocol {0}", serviceInterfaceEndpoint.EndpointName), ex);
            }
        }

        private static bool LegalEndpoint(Type interfaceType, string endpointName)
        {
            if (interfaceType.Name == "IServiceManagement" && endpointName.EqualsCaseInsensitive("rest"))
                return false;
            return true;
        }

        private static ServiceEndpoint CreateEndpoint(Binding binding, EndpointAddress address, ContractDescription cd, Endpoint serviceInterface)
        {
            if (!serviceInterface.EndpointName.EqualsCaseInsensitive("rest") && !serviceInterface.EndpointName.EqualsCaseInsensitive("securerest"))
                return new ServiceEndpoint(cd, binding, address);
            var restEndpoint = new WebHttpEndpoint(cd, address)
                       {
                           HelpEnabled = true,
                           DefaultOutgoingResponseFormat = WebMessageFormat.Json,
                           Binding = binding
                       };
            restEndpoint.AutomaticFormatSelectionEnabled = true;
            restEndpoint.FaultExceptionEnabled = true;
            return restEndpoint;
        }

        private static void AddEndpoint(ServiceHostBase selfConfiguringHost, EndpointAddress address, ServiceEndpoint se)
        {
            try
            {
                foreach (var operation in se.Contract.Operations.Where(operation => !operation.Behaviors.Contains(typeof(StardustOperationAttribute))))
                {
                    operation.Behaviors.Add(new StardustOperationAttribute());
                    operation.AddFaultContract();
                }
                selfConfiguringHost.AddServiceEndpoint(se);
            }
            catch (Exception)
            {
                throw new StardustCoreException("Error creating endpoint: " + address);
            }
        }



        private IEnumerable<Endpoint> GetEndpoints(string serviceName)
        {
            try
            {
                var endpoints = from ep in GetRuntime().Context
                                .GetEndpointConfiguration(serviceName).Endpoints
                                where ServiceHostHelper.EndpointIsSupported(ep.BindingType)
                                select ep;
                return endpoints;
            }
            catch (Exception ex)
            {

                throw new StardustCoreException(string.Format("Failed to retrieve endpoints for service {0}", serviceName), ex);
            }
        }
    }
}