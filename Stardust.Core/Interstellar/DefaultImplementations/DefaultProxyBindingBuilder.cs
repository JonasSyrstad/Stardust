//
// DefaultProxyBindingBuilder.cs
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
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;
using Stardust.Interstellar.ConfigurationReader;

namespace Stardust.Interstellar.DefaultImplementations
{
    public class DefaultProxyBindingBuilder : IProxyBindingBuilder
    {
        IRuntimeContext Context;

        public IProxyBindingBuilder SetRuntimeContext(IRuntimeContext context)
        {
            Context = context;
            return this;
        }

        public ServiceEndpoint GetServiceBinding<T>(string serviceName, string url)
        {
            var cd = GetContractDescription<T>();
            var endpoint = GetEndpointConfig(serviceName);
            return new ServiceEndpoint(cd, ServiceHostHelper.CreateBinding(endpoint.First()), GetEndpointAddress<T>(endpoint.First().Address));
        }

        private IEnumerable<Endpoint> GetEndpointConfig(string serviceName)
        {
            var endpoint = from e in Context.GetEndpointConfiguration(serviceName).Endpoints
                           where e.EndpointName == Context.GetEndpointConfiguration(serviceName).ActiveEndpoint
                           select e;
            return endpoint;
        }

        public ContractDescription GetContractDescription<T>()
        {
            var cd = ContractDescription.GetContract(typeof(T));
            return cd;
        }

        public Binding GetBinding(string serviceName, string type)
        {
            if (type.EqualsCaseInsensitive("ws"))
                return new WSHttpBinding
                           {
                               Name = "WS",
                               AllowCookies = false,
                               TransactionFlow = false,
                               HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                               MaxBufferPoolSize = 2147483647,
                               MaxReceivedMessageSize = 2147483647,
                               MessageEncoding = WSMessageEncoding.Mtom,
                               TextEncoding = Encoding.UTF8,
                               ReaderQuotas = XmlDictionaryReaderQuotas.Max,
                               BypassProxyOnLocal = true,
                               UseDefaultWebProxy = false
                           };
            return new BasicHttpBinding
            {
                Name = "basic",
                AllowCookies = false,
                HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                MaxBufferPoolSize = 2147483647,
                MaxReceivedMessageSize = 2147483647,
                MessageEncoding = WSMessageEncoding.Mtom,
                TextEncoding = Encoding.UTF8,
                ReaderQuotas = XmlDictionaryReaderQuotas.Max,
                BypassProxyOnLocal = true,
                UseDefaultWebProxy = false
            };
        }

        public EndpointAddress GetEndpointAddress<T>(string url)
        {
            url = string.Format(url, Utilities.Utilities.GetEnvironment(), Utilities.Utilities.GetVersion());
            try
            {
                return new EndpointAddress(url);
            }
            catch (Exception ex)
            {
                Logging.DebugMessage("trying to create endpoint: {0}", url);
                throw ex.LogAndRethrow();
            }
        }

        public EndpointAddress GetEndpointAddress(string serviceName)
        {   
            var endpoint = GetEndpointConfig(serviceName).First();
            try
            {
                return new EndpointAddress(endpoint.Address);
            }
            catch (Exception ex)
            {
                Logging.DebugMessage("trying to create endpoint: {0}", endpoint.Address);
                throw ex.LogAndRethrow();
            }
        }
    }
}