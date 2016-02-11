//
// BindingFactory.cs
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
using System.ServiceModel.Channels;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Nucleus;

namespace Stardust.Interstellar.Endpoints
{
    public static class BindingFactory
    {
        

        public static void Initialize() { }

        public static void BindNew<T>(string bindingType) where T : IBindingCreator
        {
            Resolver.GetConfigurator().Bind<IBindingCreator>().To<T>(bindingType);
        }

        public static Binding CreateBinding(Endpoint serviceInterface)
        {
            var binding = Resolver.Activate<IBindingCreator>(BindingName(serviceInterface)).Create(serviceInterface);
            AddTimeoutSettings(binding, serviceInterface);
            
            return binding;
        }

        private static void AddTimeoutSettings(Binding binding, Endpoint serviceInterface)
        {
            binding.CloseTimeout =
                serviceInterface.CloseTimeout == 0
                    ? TimeSpan.FromMinutes(2)
                    : TimeSpan.FromSeconds(serviceInterface.CloseTimeout);
            binding.OpenTimeout = serviceInterface.OpenTimeout == 0
                ? TimeSpan.FromMinutes(2)
                : TimeSpan.FromSeconds(serviceInterface.OpenTimeout);
            binding.ReceiveTimeout = serviceInterface.ReceiveTimeout == 0
                ? TimeSpan.FromMinutes(2)
                : TimeSpan.FromSeconds(serviceInterface.ReceiveTimeout);
            binding.SendTimeout = serviceInterface.SendTimeout == 0
                ? TimeSpan.FromMinutes(2)
                : TimeSpan.FromSeconds(serviceInterface.SendTimeout);
        }

        private static string BindingName(Endpoint serviceInterface)
        {
            return serviceInterface.BindingType;
        }
    }
}
