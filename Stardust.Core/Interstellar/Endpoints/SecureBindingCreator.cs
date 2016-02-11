//
// SecureBindingCreator.cs
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
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Text;
using System.Xml;
using JetBrains.Annotations;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;

namespace Stardust.Interstellar.Endpoints
{
    [UsedImplicitly]
    internal class SecureBindingCreator : IBindingCreator
    {
        private bool IsClient;

        public Binding Create(Endpoint serviceInterface)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                ((sender, certificate, chain, sslPolicyErrors) => true);
            var secureBinding = new WS2007FederationHttpBinding(WSFederationHttpSecurityMode.TransportWithMessageCredential)
                                    {
                                        Name = "secure",
                                        TransactionFlow = false,
                                        HostNameComparisonMode = serviceInterface.HostNameComparisonMode.ParseAsEnum(HostNameComparisonMode.StrongWildcard),
                                        MaxBufferPoolSize = serviceInterface.MaxBufferPoolSize,
                                        MaxReceivedMessageSize = serviceInterface.MaxReceivedSize,
                                        MessageEncoding = serviceInterface.MessageFormat.ParseAsEnum(WSMessageEncoding.Text),
                                        TextEncoding = Encoding.UTF8,
                                        ReaderQuotas = XmlDictionaryReaderQuotas.Max,
                                        BypassProxyOnLocal = true,
                                        UseDefaultWebProxy = false
                                    };
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.UseDefaultProxy")=="true")
            {
                secureBinding.BypassProxyOnLocal = false;
                secureBinding.UseDefaultWebProxy = true; 
            }
            SetSecuritySettings(serviceInterface, secureBinding);
            return secureBinding;
        }

        private void SetSecuritySettings(Endpoint serviceInterface, WSFederationHttpBinding secureBinding)
        {
            GetStsSettingsFromEnvironment(serviceInterface);
            secureBinding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Basic256;
            secureBinding.Security.Message.NegotiateServiceCredential = false;
            secureBinding.Security.Message.EstablishSecurityContext = false;
            secureBinding.Security.Message.IssuedKeyType = SecurityKeyType.BearerKey;
            secureBinding.Security.Message.IssuedTokenType = null;
            var identity = EndpointIdentity.CreateDnsIdentity((new Uri(serviceInterface.IssuerName).DnsSafeHost));
            secureBinding.Security.Message.IssuerAddress = CreateIssuerAddress(serviceInterface, identity);
            if (IsClient) secureBinding.Security.Message.IssuerBinding = CreateIssuerBinding(serviceInterface);
            else
                secureBinding.Security.Message.IssuerMetadataAddress = new EndpointAddress(new Uri(serviceInterface.IssuerMetadataAddress), identity);
            
        }

        private EndpointAddress CreateIssuerAddress(Endpoint serviceInterface, EndpointIdentity identity)
        {
            GetStsSettingsFromEnvironment(serviceInterface);
            if (IsClient)
                return new EndpointAddress(new Uri(serviceInterface.IssuerActAsAddress));
            return new EndpointAddress(new Uri(serviceInterface.IssuerName));

        }

        private static void GetStsSettingsFromEnvironment(Endpoint serviceInterface)
        {
            var issuerName =
                RuntimeFactory.CreateRuntime().Context.GetEnvironmentConfiguration().GetConfigParameter("IssuerName");
            if (issuerName.ContainsCharacters())
                serviceInterface.IssuerName = issuerName;
            var issuerActAsAddress =
                RuntimeFactory.CreateRuntime().Context.GetEnvironmentConfiguration().GetConfigParameter("IssuerActAsAddress");
            if (issuerActAsAddress.ContainsCharacters())
                serviceInterface.IssuerActAsAddress = issuerActAsAddress;
        }

        private static Binding CreateIssuerBinding(Endpoint serviceInterface)
        {
            var creator = new StsBindingCreator();
            return creator.SetClientMode(false).Create(serviceInterface);
        }

        public IBindingCreator SetClientMode(bool isClient)
        {
            IsClient = isClient;
            return this;
        }
    }
}