//
// ServiceProxyBuilder.cs
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
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    public static class ServiceProxyBuilder
    {
        private static Action<ChannelFactory> behaviorHandler;

        public static void AddClientBehaviorExtentions(Action<ChannelFactory> handler)
        {
            behaviorHandler = handler;
        }
        internal static ChannelFactory<TService> CreateChannelFactory<TService>(IRuntimeContext context, string servicename, string uri, bool isSecureRest) where TService : class
        {
            var scope = GetScope<TService>();
            return (ChannelFactory<TService>)ContainerFactory.Current.Resolve(typeof(ChannelFactory<TService>), scope, () => CreateInstance<TService>(context, servicename, uri, isSecureRest));
        }

        private static Scope GetScope<T>()
        {
            var ServiceName = typeof(T).GetAttribute<ServiceNameAttribute>();
            if (ServiceName.IsNull()) return Scope.Singleton;
            return ServiceName.ChannelFactoryScope;
        }

        private static ChannelFactory<TService> CreateInstance<TService>(IRuntimeContext context, string servicename, string uri, bool isSecureRest) where TService : class
        {
            var builder = Resolver.Activate<IProxyBindingBuilder>().SetRuntimeContext(context);

            var channelFactory = Create<TService>(servicename, uri, builder);
            if (behaviorHandler != null) behaviorHandler(channelFactory);
            if (isSecureRest)
                channelFactory.Endpoint.Behaviors.Add(new RestTokenHandler(servicename));
            // if (!(channelFactory.Endpoint.Binding is WebHttpBinding))
            {
                foreach (OperationDescription op in channelFactory.Endpoint.Contract.Operations)
                {

                    var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
                    if (dataContractBehavior != null)
                    {
                        dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                    }
                }
            }
            if (channelFactory.Endpoint.Binding is WebHttpBinding)
            {
                Resolver.Activate<WebBehaviorProvider>().ApplyBehavior(channelFactory.Endpoint);
                
            }
            return channelFactory;
        }

        private static ChannelFactory<TService> Create<TService>(string servicename, string uri, IProxyBindingBuilder builder) where TService : class
        {
            var binding = builder.GetServiceBinding<TService>(servicename, uri);
            if (binding.Binding is WebHttpBinding)
                return new WebChannelFactory<TService>(binding);
            return new ChannelFactory<TService>(binding);
        }

        internal static TService CreateProxy<TService>(ChannelFactory<TService> channelFactory, string uri, string servicename)
        {
            return channelFactory.CreateChannel(Utilities.Utilities.FormatAddress(uri, servicename));
        }

        public static void CloseProxy<TService>(this ServiceContainer<TService> serviceContainer)
        {
            var proxy = serviceContainer.GetClient() as IClientChannel;
            if (proxy.IsNull()) return;
            if (proxy.State == CommunicationState.Faulted)
                proxy.Abort();
            else
            {
                if (proxy.State != CommunicationState.Closed)
                    proxy.Close();
            }
            proxy.TryDispose();
        }
    }

    
}