//
// servicesbindingconfiguration.cs
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
using System.Diagnostics;
using System.ServiceModel.Description;
using Stardust.Core;
using Stardust.Core.Wcf;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.DefaultImplementations;
using Stardust.Interstellar.Utilities;
using Stardust.Nucleus;
using Stardust.Nucleus.ObjectActivator;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    /// <summary>
    /// Binds the services framework with the default logging implementation
    /// </summary>
    public class Blueprint : Blueprint<LoggingDefaultImplementation>
    {

    }

    /// <summary>
    /// Binds the services framework with a user defined logging implementation
    /// </summary>
    public class Blueprint<T> : IBlueprint where T : ILogging
    {
        private readonly CoreFrameworkBlueprint<T> BaseBlueprint = new CoreFrameworkBlueprint<T>();
        protected IConfigurator Resolver;
        protected IConfigurator Configurator { get { return Resolver; } }

        protected void UseDefaultServiceTearDown()
        {
            SetServiceTearDown<DefaultServiceTearDown>();
        }

        /// <summary>
        /// Set your own implementation of <see cref="IServiceTearDown"/>. Calling this will unbind any existing bindings.
        /// </summary>
        /// <typeparam name="T">The implementation to use.</typeparam>
        protected void SetServiceTearDown<T>() where T : IServiceTearDown
        {
            Resolver.UnBind<IServiceTearDown>()
                .AllAndBind()
                .To<T>()
                .SetSingletonScope();
        }

        /// <summary>
        /// Override this to change add custom Stardust.Core.Services bindings. 
        /// </summary>
        /// <remarks>
        /// If overridden the following needs to be bound:
        /// <see cref="IConfigurationReader" /> 
        ///  <see cref="IUrlFormater"/>
        /// and <see cref="IServiceTearDown"/>
        /// </remarks>
        protected virtual void SetDefaultBindings()
        {
            Bind<IConfigurationReader, StarterkitConfigurationReader>().SetSingletonScope().AllowOverride = false;
            Bind<IUrlFormater, UrlFormater>().SetSingletonScope().AllowOverride = false;
            UseDefaultServiceTearDown();
        }

        internal void FrameworkBindings()
        {
            ActivatorFactory.ResetActivator();
            Configurator.Bind<WebBehaviorProvider>().To<WebBehaviorProvider>().SetTransientScope();
            Bind<IServiceHostBehaviour, ServiceHostBehaviorConfiguration>();
            Bind<IRuntime, Runtime>().SetRequestResponseScope().AllowOverride = true;
            Bind<IRuntime, Runtime>(Scope.PerRequest).SetTransientScope().AllowOverride = true;
            Bind<IRuntimeContext, DefaultRuntimeContext>().SetTransientScope().AllowOverride = true;
            Bind<IProxyBindingBuilder, DefaultProxyBindingBuilder>().SetTransientScope().DisableOverride();
            Bind<IServiceConfiguration, ConfigureEndpoints>();
            Resolver.Bind<IEndpointUrlFormater>().To<EndpointUrlFormater>();
            Resolver.Bind<IServiceBehavior>().To<ModuleCreatorBehaviourAttribute>("IOC").SetTransientScope();
            SetDefaultBindings();
            Logging.DebugMessage("Stardust default bindings added.", EventLogEntryType.SuccessAudit);
            DoCustomBindings();
            Logging.DebugMessage("Custom bindings added.", EventLogEntryType.SuccessAudit);
        }


        /// <summary>
        /// Place your bindings here
        /// </summary>
        protected virtual void DoCustomBindings()
        {
        }

        /// <summary>
        /// creates a context aware binding.
        /// </summary>
        /// <param name="name">the name of the binding, to be used when resolving the implementation runtime</param>
        /// <typeparam name="TBase"><see langword="interface"/> or base class to bind to an implementation</typeparam>
        /// <typeparam name="TImplementation">The implementation to use at runtime</typeparam>
        /// <returns>a scope context for declaring the scope of the instance</returns>
        protected IScopeContext Bind<TBase, TImplementation>(string name) where TImplementation : TBase
        {
            return Resolver.Bind<TBase>().To<TImplementation>(name);
        }

        /// <summary>
        /// creates a context aware binding.
        /// </summary>
        /// <param name="type">the type of the binding, to be used when resolving the implementation runtime</param>
        /// <typeparam name="TBase"><see langword="interface"/> or <see langword="base"/> class to bind to an implementation</typeparam>
        /// <typeparam name="TImplementation">The implementation to use at runtime</typeparam>
        /// <returns>a scope context for declaring the scope of the instance</returns>
        protected IScopeContext Bind<TBase, TImplementation>(Enum type) where TImplementation : TBase
        {
            return Bind<TBase, TImplementation>(type.ToString());
        }

        /// <summary>
        /// creates a default binding.
        /// </summary>
        /// <typeparam name="TBase"><see langword="interface"/> or base class to bind to an implementation</typeparam>
        /// <typeparam name="TImplementation">The implementation to use at runtime</typeparam>
        /// <returns>a scope context for declaring the scope of the instance</returns>
        protected IScopeContext Bind<TBase, TImplementation>() where TImplementation : TBase
        {
            return Bind<TBase, TImplementation>("default");
        }

        public void Bind(IConfigurator resolver)
        {
            Resolver = resolver;
            BaseBlueprint.Bind(resolver);
            FrameworkBindings();
        }

        public Type LoggingType
        {
            get { return BaseBlueprint.LoggingType; }
        }
    }


}