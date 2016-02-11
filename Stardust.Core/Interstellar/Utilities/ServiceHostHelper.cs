//
// ServiceHostHelper.cs
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

//
// ServiceHostHelper.cs
// This file is part of Pragma
//
// Author: Jonas Syrstad (jsyrstad2+PragmaCore@gmail.com), http://no.linkedin.com/in/jonassyrstad/) 
// Copyright (c) 2013 Jonas Syrstad. All rights reserved.
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
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Interstellar.Endpoints;

namespace Stardust.Interstellar.Utilities
{
    /// <summary>
    /// Class with some helper methods for creating endpoints and service hosts. Only for internal use.
    /// </summary>
    public static class ServiceHostHelper
    {
        public static Binding CreateBinding(Endpoint serviceInterface)
        {
            try
            {
                return BindingFactory.CreateBinding(serviceInterface);
            }
            catch (Exception ex)
            {
                throw new InvalidChannelBindingException(string.Format("Error creating binding {0}", serviceInterface.EndpointName),ex);
            }
        }

        public static EndpointAddress EndpointAddress(string serviceName, Endpoint serviceInterface,string rootUrl)
        {
            try
            {
                var formater = Resolver.Activate<IEndpointUrlFormater>();
                return formater.GetEndpointAddress(serviceName, serviceInterface, rootUrl);
            }
            catch (Exception ex)
            {
                
                throw new FormatException("Failure formating endpoint address",ex);
            }
        }

        public static string GetServiceName(Type serviceType, IEnumerable<Type> serviceInterfaces)
        {
            var serviceName = (from type in serviceInterfaces
                               let attrib = Utilities.GetServiceNameFromAttribute(type)
                               where attrib.IsInstance()
                               select attrib.ServiceName).FirstOrDefault();
            if (serviceName.IsNullOrWhiteSpace())
                serviceName = serviceType.Name;
            return serviceName;
        }

        public static bool InterfaceIsNotServiceContract(Type type)
        {
            var attrib = type.GetAttribute<ServiceContractAttribute>();
            return attrib.IsNull();
        }

        public static bool EndpointIsSupported(string bindingName)
        {
            var disabledProtocols = LocalConfigStore.DisabledProtocols();
            if (disabledProtocols.IsNullOrEmpty()) return true;
            return !disabledProtocols.Contains(bindingName);
        }

        internal static string GetServiceRootUrl(IRuntime Runtime, Type serviceType, Type[] serviceInterfaces)
        {
            var serviceName = (from type in serviceInterfaces
                               let attrib = Utilities.GetServiceNameFromAttribute(type)
                               where attrib.IsInstance()
                               select attrib).FirstOrDefault();
            return serviceName.IsInstance() ? serviceName.UrlRootName : null;
        }
    }
}