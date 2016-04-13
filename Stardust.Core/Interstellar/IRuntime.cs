//
// iruntime.cs
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
using System.ComponentModel;
using System.Net.Http;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;
using JetBrains.Annotations;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Tasks;
using Stardust.Nucleus;
using Stardust.Interstellar.Trace;

namespace Stardust.Interstellar
{
    /// <summary>
    /// The <see cref="IRuntime"/> represents the common infrastructure for
    /// building large SOA applications. The runtime is responsible for creating
    /// and configuring tasks, service proxies (generated from the service
    /// <see langword="interface"/> or a service reference ). 
    /// </summary>
    public interface IRuntime
    {
        /// <summary>
        /// A unique id for the runtime instance within a request/responce scope (Scope.Context)
        /// </summary>
        Guid InstanceId { get; }
        /// <summary>
        /// the configuration reader abstraction. this enables access to properties stored in the config system
        /// </summary>
        IRuntimeContext Context { get; }

        /// <summary>
        /// The inncomming service request
        /// </summary>
        IRequestBase RequestContext { get; }

        /// <summary>
        /// Builds a tree structure as services and tasks are executed.
        /// </summary>
        CallStackItem CallStack { get; }

        /// <summary>
        /// The environment name from machine.config
        /// </summary>
        string Environment { get; }


        /// <summary>
        /// The service name, from web.config
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Disables the callstack builder for the current instance of the runtime.
        /// </summary>
        bool NoTrace { get; set; }

        /// <summary>
        /// To be used with generated proxies
        /// </summary>
        IServiceInitializer<TService> CreateNewService<TService>(Scope scope, string serviceName) where TService : ICommunicationObject;

        /// <summary>
        /// To be used with generated proxies
        /// </summary>
        IServiceInitializer<TService> CreateNewService<TService>(string serviceName) where TService : ICommunicationObject;

        
        /// <summary>
        /// Creates a wrapper around the service interface. This wrapper configures the endpoint and implements the dispose pattern allowing the use of "using" statements.
        /// Uses Transient OLM scope
        /// </summary>
        IServiceContainer<TService> CreateServiceProxy<TService>() where TService : class;

        /// <summary>
        /// Creates a wrapper around the service interface. This wrapper configures the endpoint and implements the dispose pattern allowing the use of "using" statements.
        /// </summary>
        IServiceContainer<TService> CreateServiceProxy<TService>(Scope scope) where TService : class;

        /// <summary>
        /// Creates a wrapper around the service interface. This wrapper configures the endpoint and implements the dispose pattern allowing the use of "using" statements.
        /// </summary>
        IServiceContainer<TService> CreateServiceProxy<TService>(string serviceName, Scope scope = Scope.Context) where TService : class;

        /// <summary>
        /// Creates a pooled service proxy. Pooled services are usefull to throttle access to a service. 
        /// </summary>
        /// <typeparam name="TService">the type of service</typeparam>
        /// <returns></returns>
        IPooledServiceContainer<TService> CreatePooledServiceProxy<TService>(string serviceName) where TService : class;

        /// <summary>
        /// Creates a pooled service proxy. Pooled services are usefull to throttle access to a service. 
        /// </summary>
        /// <typeparam name="TService">the type of service</typeparam>
        /// <returns></returns>
        IPooledServiceContainer<TService> CreatePooledServiceProxy<TService>() where TService : class;

        /// <summary>
        /// Creates a pooled service proxy asynchronously. Pooled services are usefull to throttle access to a service. 
        /// </summary>
        /// <param name="serviceName">The service name</param>
        /// <typeparam name="TService">the type of service</typeparam>
        /// <returns></returns>
        Task<IPooledServiceContainer<TService>> CreatePooledServiceProxyAsync<TService>(string serviceName) where TService : class;

        /// <summary>
        /// Creates a pooled service proxy asynchronously. Pooled services are usefull to throttle access to a service. 
        /// </summary>
        /// <typeparam name="TService">the type of service</typeparam>
        /// <returns></returns>
        Task<IPooledServiceContainer<TService>> CreatePooledServiceProxyAsync<TService>() where TService : class;

        /// <summary>
        /// Initializes the task with the incoming context
        /// </summary>
        [UsedImplicitly]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IRuntime InitializeWithMessageContext(IRequestBase message);

        /// <summary>
        /// Initializes the runtime with the specified configuration set
        /// </summary>
        [UsedImplicitly]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IRuntime InitializeWithConfigSetName(string configSet);

        /// <summary>
        /// Initializes the task with the incoming context
        /// </summary>
        [UsedImplicitly]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IRuntime InitializeWithContext(object callingInstance, HttpRequestMessage context);

        /// <summary>
        /// Initializes the task with the incoming context
        /// </summary>
        [UsedImplicitly]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IRuntime InitializeWithContext(object callingInstance, HttpRequestBase context);

        /// <summary>
        /// Initializes the task with the environment name. Used to resolve configuration settings form the config system
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        IRuntime SetEnvironment(string environment);

        /// <summary>
        /// Initializes the task with the service name. Used to resolve configuration settings from the config system.
        /// </summary>
        [UsedImplicitly]
        [EditorBrowsable(EditorBrowsableState.Never)]
        ITracer SetServiceName(object callingInstance, string serviceName, string methodName);


        /// <summary>
        /// Gets the inncomming message 
        /// </summary>
        T GetServiceRequest<T>() where T : IRequestBase;

        /// <summary>
        /// Gets the inncomming message 
        /// </summary>
        object GetServiceRequest();

        /// <summary>
        /// Get a container for variables needed whith in the same Runtime life time. Use this to store data that needs to be passed between tasks. 
        /// </summary>
        /// <returns></returns>
        IStateStorageTask GetStateStorageContainer();

        /// <summary>
        /// Returns the root tracer for the Runtime instance
        /// </summary>
        /// <returns>the tracer</returns>
        ITracer GetTracer();
    }
}