//
// RestBindingCreator.cs
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

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using JetBrains.Annotations;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;

namespace Stardust.Interstellar.Endpoints
{
    [UsedImplicitly]
    public class RestBindingCreator : IBindingCreator
    {
        private bool clientMode;

        public Binding Create(Endpoint serviceInterface)
        {
            var binding = new WebHttpBinding(WebHttpSecurityMode.Transport)
                       {
                           AllowCookies = false,
                           HostNameComparisonMode = serviceInterface.HostNameComparisonMode.ParseAsEnum(HostNameComparisonMode.WeakWildcard),
                           MaxBufferPoolSize = serviceInterface.MaxBufferPoolSize,
                           MaxReceivedMessageSize = serviceInterface.MaxReceivedSize,
                           ReaderQuotas = XmlDictionaryReaderQuotas.Max,
                           BypassProxyOnLocal = true,
                           UseDefaultWebProxy = false,
                           MaxBufferSize = serviceInterface.MaxBufferSize
                       };
            
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.UseDefaultProxy") == "true")
            {
                binding.BypassProxyOnLocal = false;
                binding.UseDefaultWebProxy = true;
            }
            return binding;
        }

        public IBindingCreator SetClientMode(bool isClient)
        {
            clientMode = isClient;
            return this;
        }
    }
    public class SecureWebHttpBinding:WebHttpBinding
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.ServiceModel.WebHttpBinding"/> class. 
        /// </summary>
        public SecureWebHttpBinding()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.ServiceModel.WebHttpBinding"/> class with a binding specified by its configuration name.
        /// </summary>
        /// <param name="configurationName">The binding configuration name for the <see cref="T:System.ServiceModel.Configuration.WebHttpBindingElement"/>.</param><exception cref="T:System.Configuration.ConfigurationErrorsException">The binding element with the name <paramref name="configurationName"/> was not found.</exception>
        public SecureWebHttpBinding([NotNull] string configurationName)
            : base(configurationName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.ServiceModel.WebHttpBinding"/> class with the type of security used by the binding explicitly specified.
        /// </summary>
        /// <param name="securityMode">The value of <see cref="T:System.ServiceModel.WebHttpSecurityMode"/> that specifies the type of security that is used to configure a service endpoint to receive HTTP requests.</param><exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="securityMode"/> specified is not a valid <see cref="T:System.ServiceModel.WebHttpSecurityMode"/>.</exception>
        public SecureWebHttpBinding(WebHttpSecurityMode securityMode)
            : base(securityMode)
        {
        }
    }

    [UsedImplicitly]
    public class SecureRestBindingCreator : IBindingCreator
    {
        public Binding Create(Endpoint serviceInterface)
        {
            var binding = new SecureWebHttpBinding(WebHttpSecurityMode.Transport)
            {
                AllowCookies = false,
                HostNameComparisonMode = serviceInterface.HostNameComparisonMode.ParseAsEnum(HostNameComparisonMode.WeakWildcard),
                MaxBufferPoolSize = serviceInterface.MaxBufferPoolSize,
                MaxReceivedMessageSize = serviceInterface.MaxReceivedSize,
                ReaderQuotas = XmlDictionaryReaderQuotas.Max,
                BypassProxyOnLocal = true,
                UseDefaultWebProxy = false,
                MaxBufferSize = serviceInterface.MaxBufferSize
            };
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.UseDefaultProxy") == "true")
            {
                binding.BypassProxyOnLocal = false;
                binding.UseDefaultWebProxy = true;
            }
            
            return binding;
        }

        public IBindingCreator SetClientMode(bool isClient)
        {
            return this;
        }
    }
}