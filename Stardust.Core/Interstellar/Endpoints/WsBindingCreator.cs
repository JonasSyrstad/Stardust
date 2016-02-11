//
// WsBindingCreator.cs
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
using System.Text;
using System.Xml;
using JetBrains.Annotations;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;

namespace Stardust.Interstellar.Endpoints
{
    [UsedImplicitly]
    public class WsBindingCreator : IBindingCreator
    {
        public virtual Binding Create(Endpoint serviceInterface)
        {
            var binding = new WSHttpBinding(SecurityMode.Transport)
                              {
                                  Name = serviceInterface.EndpointName,
                                  AllowCookies = false,
                                  TransactionFlow = false,
                                  HostNameComparisonMode = serviceInterface.HostNameComparisonMode.ParseAsEnum(HostNameComparisonMode.StrongWildcard),
                                  MaxBufferPoolSize = serviceInterface.MaxBufferPoolSize,
                                  MaxReceivedMessageSize = serviceInterface.MaxReceivedSize,
                                  MessageEncoding = serviceInterface.MessageFormat.ParseAsEnum(WSMessageEncoding.Text),
                                  TextEncoding = Encoding.UTF8,
                                  ReaderQuotas = XmlDictionaryReaderQuotas.Max,
                                  BypassProxyOnLocal = true,
                                  UseDefaultWebProxy = false,
                                  
                              };
            binding.Security.Transport.ClientCredentialType=HttpClientCredentialType.None;
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