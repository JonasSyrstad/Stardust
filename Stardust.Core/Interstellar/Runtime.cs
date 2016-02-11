//
// runtime.cs
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
using System.Collections.Concurrent;
using System.Net.Http;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Tasks;
using Stardust.Interstellar.Trace;
using Stardust.Nucleus;
using Stardust.Nucleus.Internals;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    internal sealed class Runtime : IRuntime
    {
        private const string CreatingContextFailed = "Creating Runtime context failed: {0}";

        private readonly ConcurrentDictionary<string, IRuntimeContext> Contexts = new ConcurrentDictionary<string, IRuntimeContext>();
        private string _configSetName;
        private string Name;
        private StateStorageTask StateContainer;
        private static readonly object Triowing = new object();

        public Runtime()
        {
            InstanceId = RuntimeFactory.GetInstanceId();

        }

        public string ConfigSetName
        {
            get
            {
                if (_configSetName.IsNullOrWhiteSpace()) return Utilities.Utilities.GetConfigSetName();
                return _configSetName;
            }
            private set
            {
                _configSetName = value;
                if (Contexts.ContainsKey(value))
                    Contexts[value].SetConfigSet(value);
            }
        }
        public CallStackItem CallStack { get; private set; }

        public Guid InstanceId { get; private set; }

        public IRuntimeContext Context
        {
            get
            {
                IRuntimeContext configSet;
                if (Contexts.TryGetValue(ConfigSetName, out configSet)) return configSet;
                configSet = Resolver.Activate<IRuntimeContext>();
                Contexts.TryAdd(ConfigSetName, configSet);
                configSet.SetConfigSet(ConfigSetName);
                configSet.SetServiceName(ServiceName);
                return configSet;
            }
        }

        public string Environment { get; private set; }

        public IRequestBase RequestContext { get; private set; }
        public string ServiceName
        {
            get
            {
                if (Name.ContainsCharacters()) return Name;
                return Utilities.Utilities.GetServiceName();
            }
            private set
            {
                Name = value;
                Context.SetServiceName(value);
            }
        }

        public ITracer Tracer { get; private set; }

        public IServiceInitializer<TService> CreateNewService<TService>(Scope scope, string serviceName) where TService : ICommunicationObject
        {
            var item = (IScopeContextInternal)Resolver.ConfigurationKernel.Resolve(typeof(IServiceInitializer<TService>), TypeLocatorNames.DefaultName);
            return item.BoundType.Activate<IServiceInitializer<TService>>(scope, o => o.Initialize(this, serviceName));
        }

        public IServiceInitializer<TService> CreateNewService<TService>(string serviceName) where TService : ICommunicationObject
        {
            return CreateNewService<TService>(Scope.Context, serviceName);
        }

        public IPooledServiceContainer<TService> CreatePooledServiceProxy<TService>()
        {
            return ServiceContainerFactory.CreatePooledServiceContainer<TService>(this);
        }

        public IPooledServiceContainer<TService> CreatePooledServiceProxy<TService>(string serviceName)
        {
            return ServiceContainerFactory.CreatePooledServiceProxy<TService>(this, serviceName);
        }

        public Task<IPooledServiceContainer<TService>> CreatePooledServiceProxyAsync<TService>()
        {
            return RuntimeFactory.Run(() => CreatePooledServiceProxy<TService>());
        }

        public Task<IPooledServiceContainer<TService>> CreatePooledServiceProxyAsync<TService>(string serviceName)
        {
            return RuntimeFactory.Run(() => CreatePooledServiceProxy<TService>(serviceName));
        }

        public TTask CreateRuntimeTask<TTask>() where TTask : IRuntimeTask
        {
            return CreateRuntimeTask<TTask>(ObjectInitializer.Default.Name, ScopeContext.GetDefaultScope());
        }

        public TTask CreateRuntimeTask<TTask>(Scope scope) where TTask : IRuntimeTask
        {
            return CreateRuntimeTask<TTask>(ObjectInitializer.Default.Name, scope);
        }

        public TTask CreateRuntimeTask<TTask>(Enum implementationRef) where TTask : IRuntimeTask
        {
            return CreateRuntimeTask<TTask>(implementationRef.ToString(), ScopeContext.GetDefaultScope());
        }

        public TTask CreateRuntimeTask<TTask>(Enum implementationRef, Scope scope) where TTask : IRuntimeTask
        {
            return CreateRuntimeTask<TTask>(implementationRef.ToString(), scope);
        }

        public TTask CreateRuntimeTask<TTask>(string implementationRef) where TTask : IRuntimeTask
        {
            return CreateRuntimeTask<TTask>(implementationRef, ScopeContext.GetDefaultScope());
        }

        /// <summary>
        /// Creates and initializes a new IRuntimeTask implementation. 
        /// </summary>
        /// <param name="implementationRef"></param>
        /// <param name="scope">Deprecated, no longer in use. Defined on binding</param>
        public TTask CreateRuntimeTask<TTask>(string implementationRef, Scope scope) where TTask : IRuntimeTask
        {
            IRuntime runtime = this;
            var instance = Resolver.Activate<TTask>(implementationRef, t => InitializeTask(t, runtime));
            return instance;
        }

        public IServiceContainer<TService> CreateServiceProxy<TService>()
        {
            return CreateServiceProxy<TService>(Scope.PerRequest);
        }

        public IServiceContainer<TService> CreateServiceProxy<TService>(Scope scope)
        {
            var name = Utilities.Utilities.GetServiceNameFromAttribute(typeof(TService));
            if (name.IsInstance()) return CreateServiceProxy<TService>(name.ServiceName, scope);
            return CreateServiceProxy<TService>(Context.GetClientProxyBindingName<TService>(), scope);
        }

        public IServiceContainer<TService> CreateServiceProxy<TService>(string serviceName, Scope scope = Scope.Context)
        {
            return ServiceContainerFactory.CreateContainer<TService>(this, serviceName, scope);
        }


        public object GetServiceRequest()
        {
            return RequestContext;
        }

        public T GetServiceRequest<T>() where T : IRequestBase
        {
            if (RequestContext is T)
                return (T)RequestContext;
            return default(T);
        }

        public IStateStorageTask GetStateStorageContainer()
        {
            if (StateContainer.IsNull()) StateContainer = new StateStorageTask(this);
            return StateContainer;
        }

        public ITracer GetTracer()
        {
            return Tracer;
        }

        public IRuntime InitializeWithContext(object callingInstance, HttpRequestMessage context)
        {
            SetServiceName(callingInstance, String.Format("{0}://{1}", context.RequestUri.Scheme, context.RequestUri.Host), context.Method.Method);
            return this;
        }

        public IRuntime InitializeWithContext(object callingInstance, HttpRequestBase context)
        {
            SetServiceName(callingInstance, String.Format("{0}://{1}", context.Url.Scheme, context.Url.Host), context.HttpMethod);
            return this;
        }

        public IRuntime InitializeWithMessageContext(IRequestBase message)
        {
            if (message.RequestHeader.IsNull()) message.RequestHeader = new RequestHeader();
            InitializeWithConfigSetName(message.RequestHeader.ConfigSet);
            RequestContext = message;
            return this;
        }

        public IRuntime InitializeWithConfigSetName(string configSet)
        {
            if (configSet.ContainsCharacters())
                ConfigSetName = configSet;
            return this;
        }

        public IRuntime SetEnvironment(string environment)
        {
            Environment = environment;
            return this;
        }

        public ITracer SetServiceName(object callingInstance, string serviceName, string methodName)
        {
            ServiceName = serviceName;
            if (Tracer.IsInstance() && !Tracer.IsDisposed) return Tracer;
            Tracer = TracerFactory.StartTracer(callingInstance, serviceName, methodName);
            CallStack = Tracer.GetCallstack();
            return Tracer;
        }

        public bool NoTrace { get; set; }

        private static void InitializeTask(IRuntimeTask task, IRuntime runtime)
        {
            task.SetExternalState(ref runtime)
                .SetInvokerStateStorage(runtime.GetStateStorageContainer());
        }


    }
}