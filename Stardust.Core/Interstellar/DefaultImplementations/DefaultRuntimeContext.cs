//
// defaultruntimecontext.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Serializers;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar.DefaultImplementations
{
    [BoundScope(Scope.Singleton)]
    public class DefaultRuntimeContext : IRuntimeContext
    {
        private static readonly Dictionary<string, Scope> ScopeResolver = new Dictionary<string, Scope>();

        private static readonly object TrioWing = new object();
        private static ConcurrentDictionary<string, EndpointConfig> endpointCache = new ConcurrentDictionary<string, EndpointConfig>();
        private static ConcurrentDictionary<string, EnvironmentConfig> environmentCache = new ConcurrentDictionary<string, EnvironmentConfig>();
        private static ConcurrentDictionary<string, ServiceConfig> serviceCache = new ConcurrentDictionary<string, ServiceConfig>();
        private ConfigurationSet ConfigSet;

        private string ConfigSetName;
        private string ServiceName;
        public bool ConfigSetLoaded
        {
            get
            {
                return ConfigSet != null;
            }
        }

        public Exception LastException { get; private set; }
        public bool RecycleOnChange { get; set; }
        public void BindSimpeTaskScope<T>(Scope scope) where T : IRuntimeTask
        {
            if (ScopeResolver.ContainsKey(typeof(T).FullName))
                ScopeResolver.Add(typeof(T).FullName, scope);
        }

        public string GetClientProxyBindingName<T>(string serviceName = "")
        {
            var type = typeof(T);
            var val = GetConfigParameter(type.Name);
            if (val.ContainsCharacters()) return val;
            return type.FullName;
        }

        public ConfigParameter GetComplexConfigurationParameters(string rootName)
        {
            try
            {
                var cp = from c in ConfigSet.Parameters where c.Name == rootName select c;
                return cp.First();
            }
            catch (Exception ex)
            {
                if (LastException != null)
                    LastException.Log();
                throw new InvalidDataException(string.Format("Cannot find Parameters node with name {0}", rootName), LastException ?? ex);
            }
        }

        public string GetConfigParameter(string name)
        {
            if (ConfigSet == null) return ConfigurationManagerHelper.GetValueOnKey(name);
            var envVal = GetEnvironmentConfiguration().GetConfigParameter(name);
            return envVal.ContainsCharacters() ? envVal : ConfigurationManagerHelper.GetValueOnKey(name);
        }

        public string GetConfigParameter(string name, string defaultValue)
        {
            var val = GetConfigParameter(name);
            if (val.ContainsCharacters()) return val;
            return defaultValue;
        }

        public string GetConnectionString(string name)
        {
            var envVal = GetEnvironmentConfiguration().GetConfigParameter("db_" + name);
            if (envVal.ContainsCharacters()) return envVal;
            return ConfigurationManagerHelper.GetConnectionStringOnKey(name).ConnectionString;
        }

        public string GetDefaultConnectionString()
        {
            return GetConnectionString("default");
        }

        public EndpointConfig GetEndpointConfiguration(string serviceName)
        {
            try
            {
                EndpointConfig ep;
                if (endpointCache.TryGetValue(serviceName, out ep)) return ep;
                var endpoint = from e in ConfigSet.Endpoints where e.ServiceName == serviceName select e;
                ep = endpoint.First();
                endpointCache.TryAdd(serviceName, ep);
                return ep;
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(string.Format("Cannot find endpoint node with ServiceName = {0}", serviceName), LastException ?? ex);
            }
        }

        public virtual EndpointConfig GetEndpointConfiguration()
        {
            if (ServiceName.IsNullOrWhiteSpace())
                return GetEndpointConfiguration(Utilities.Utilities.GetServiceName());
            return GetEndpointConfiguration(ServiceName);
        }

        public EndpointConfig GetEndpointConfiguration<TService>()
        {
            var name = Utilities.Utilities.GetServiceNameFromAttribute<TService>().ServiceName;
            return GetEndpointConfiguration(name);
        }

        public EnvironmentConfig GetEnvironmentConfiguration(string environmentName)
        {
            try
            {
                EnvironmentConfig env;
                if (environmentCache.TryGetValue(environmentName, out env)) return env;
                if (ConfigSet.IsNull()) LoadConfigSet();
                var envs = from e in ConfigSet.Environments where e.EnvironmentName == environmentName select e;
                env = envs.First();
                environmentCache.TryAdd(environmentName, env);
                return env;
            }
            catch (Exception ex)
            {
                if (LastException != null)
                    LastException.Log();
                throw new InvalidDataException(string.Format("Cannot find environment node with name = {0}", environmentName), LastException ?? ex);
            }
        }

        public EnvironmentConfig GetEnvironmentConfiguration()
        {
            return GetEnvironmentConfiguration(Utilities.Utilities.GetEnvironment());
        }

        /// <summary>
        /// Reads a parameter value and tries to deserialize if from json into T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">the name of the config parameter</param>
        /// <returns>the deserialized object or the default value</returns>
        public T GetGetConfigParameter<T>(string name)
        {
            try
            {
                var value = GetConfigParameter(name);
                if (value.ContainsCharacters()) return Resolver.Activate<IReplaceableSerializer>().Deserialize<T>(value);
                return default(T);
            }
            catch (Exception ex)
            {
                ex.Log(string.Format("Invalid configuration parameter '{0}', unable to serialize", name));
                return default(T);
            }
        }

        public string GetLocalConfigurationValue(string key)
        {
            return ConfigurationManagerHelper.GetValueOnKey(key);
        }

        public T GetRawConfigData<T>(string setName)
        {
            if (ConfigSet is T)
                return ConfigSet.Cast<T>();
            return default(T);
        }

        public object GetRawConfigData(string setName)
        {
            return ConfigSet;
        }

        public ServiceConfig GetServiceConfiguration(string serviceName)
        {
            try
            {
                ServiceConfig service;
                if (serviceCache.TryGetValue(serviceName, out service)) return service;
                var services = from s in ConfigSet.Services where s.ServiceName == serviceName select s;
                service = services.First();
                serviceCache.TryAdd(serviceName, service);
                return service;
            }
            catch (Exception ex)
            {
                if (LastException != null)
                    LastException.Log();
                throw new InvalidDataException(string.Format("Cannot find service node with name = {0}", serviceName), LastException ?? ex);
            }
        }

        public ServiceConfig GetServiceConfiguration()
        {
            return GetServiceConfiguration(ServiceName.IsNullOrWhiteSpace() ? Utilities.Utilities.GetServiceName() : ServiceName);
        }

        public Scope GetTaskDefaultScope()
        {
            try
            {
                return GetConfigParameter("DefaultScope", "PerRequest").ParseAsEnum<Scope>();
            }
            catch
            {
                return Scope.PerRequest;
            }
        }

        public Scope GetTaskScope(string taskName)
        {
            if (!ScopeResolver.ContainsKey(taskName))
                return GetTaskDefaultScope();
            return ScopeResolver[taskName];
        }

        public void SaveConfigurationSet()
        {
            ReaderFactory.GetConfigurationReader().WriteConfigurationSet(ConfigSet, Utilities.Utilities.GetConfigSetName());
        }

        public IRuntimeContext SetConfigSet(string value)
        {
            ConfigSetName = value;
            LoadConfigSet();
            return this;
        }

        public void SetServiceName(string serviceName)
        {
            if (serviceName.IsNullOrWhiteSpace())
                ServiceName = Utilities.Utilities.GetServiceName();
            else if (serviceName.StartsWith("http://") || serviceName.StartsWith("https://"))
                ServiceName = Utilities.Utilities.GetServiceName();
            else
                ServiceName = serviceName;
        }
        public bool TryRegisterService(ServiceRegistrationServer.ServiceRegistrationMessage serviceMessage)
        {
            try
            {
                return ReaderFactory.GetConfigurationReader().TryRegisterService(serviceMessage);
            }
            catch (Exception ex)
            {
                ex.Log();
                return false;
            }
        }

        private void LoadConfigSet()
        {
            try
            {
                var reader = ReaderFactory.GetConfigurationReader();
                reader.SetCacheInvalidatedHandler(
                    configSet =>
                        {
                            endpointCache.Clear();
                            environmentCache.Clear();
                            serviceCache.Clear();
                            ConfigSet = null;
                            ConfigSet = reader.GetConfiguration(ConfigSetName);
                        });
                ConfigSet = reader.GetConfiguration(ConfigSetName);
            }
            catch (Exception ex)
            {
                LastException = ex;
                throw;
            }
        }
    }
}