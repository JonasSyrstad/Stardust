//
// iruntimecontext.cs
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
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Nucleus;

namespace Stardust.Interstellar
{
    public delegate void EventHandler(Object sender, EventArgs e);

    /// <summary>
    /// provides a common abstraction over the configuration store. The configuration data is normalized into the Stardust configuration format.
    /// </summary>
    public interface IRuntimeContext
    {
        bool ConfigSetLoaded { get; }
        /// <summary>
        /// Set the service host name to the configuration context
        /// </summary>
        /// <param name="serviceName"></param>
        void SetServiceName(string serviceName);

        void SaveConfigurationSet();
        bool RecycleOnChange { get; }
        /// <summary>
        /// Retrieves a configuration parameter from the config system. if the item does not exist there it is picked from app/web.config
        /// </summary>
        /// <param name="name">the name of the parameter</param>
        /// <returns></returns>
        string GetConfigParameter(string name);
        /// <summary>
        /// Retrieves a configuration parameter from the config system. if the item does not exist there it is picked from app/web.config
        /// </summary>
        /// <param name="name">the name of the parameter</param>
        /// <param name="defaultValue">a value to return if not found</param>
        /// <returns></returns>
        string GetConfigParameter(string name, string defaultValue);

        T GetGetConfigParameter<T>(string name);
        ConfigParameter GetComplexConfigurationParameters(string rootName);
        ServiceConfig GetServiceConfiguration(string serviceName);
        ServiceConfig GetServiceConfiguration();
        EnvironmentConfig GetEnvironmentConfiguration(string environmentName);
        EnvironmentConfig GetEnvironmentConfiguration();
        EndpointConfig GetEndpointConfiguration<TService>();
        EndpointConfig GetEndpointConfiguration(string serviceName);
        EndpointConfig GetEndpointConfiguration();
        /// <summary>
        /// Retrieves a connection string from the config system. if the item does not exist there it is picked from app/web.config
        /// </summary>
        /// <param name="name">name of the connection string. the parameter is prefixed with 'db_' in the config system</param>
        /// <returns></returns>
        string GetConnectionString(string name);
        /// <summary>
        /// Retrieves a connection string from the config system by the name default (in web.config) or db_default (in the config system). if the item does not exist there it is picked from app/web.config
        /// </summary>
        /// <returns></returns>
        string GetDefaultConnectionString();
        string GetClientProxyBindingName<T>(string serviceName = "");
        Scope GetTaskDefaultScope();
        Scope GetTaskScope(string taskName);
        void BindSimpeTaskScope<T>(Scope scope) where T : IRuntimeTask;
        string GetLocalConfigurationValue(string key);
        T GetRawConfigData<T>(string setName = "");
        object GetRawConfigData(string setName = "");

        IRuntimeContext SetConfigSet(string value);
        bool TryRegisterService(ServiceRegistrationServer.ServiceRegistrationMessage serviceMessage);
        Exception LastException { get; }
    }
}