using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Web.Hosting;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Interstellar.Endpoints;

namespace Stardust.Interstellar
{
    /// <summary>
    /// Helper class for service registration requests.
    /// </summary>
    public static class ServiceRegistrationServer
    {
        private static string RegistrationCache = @"C:\stardust\cache\stardustServiceRegistrationSettings.ini";
        private static bool PathEnsured;

        /// <summary>
        /// Registers service definitions with the config system
        /// </summary>
        /// <typeparam name="T">The concrete implementation of the registration code</typeparam>
        public static void RegisterServices<T>() where T : IServiceRegistrationDefinition, new()
        {
            var registrar = new T();
            registrar.RegisterServices();
        }

        /// <summary>
        /// Overrides the default registration cache location.
        /// </summary>
        /// <remarks>the default is C:\temp\stardustSettings.ini</remarks>
        /// <param name="path">full file path to the cache file.</param>
        public static void SetRegistrationCacheLocation(string path)
        {
            RegistrationCache = path;
        }

        /// <summary>
        /// Registers service definitions with the config system. Uses the IoC container to locatd the registration implementation
        /// </summary>
        public static void RegisterServices()
        {
            Resolver.Activate<IServiceRegistrationDefinition>(Scope.PerRequest).RegisterServices();
        }
        public static IEnumerable<string> AvaliableBindings
        {
            get
            {
                return (from i in Resolver.GetImplementationsOf<IBindingCreator>() where i.Key != "sts" select i.Key).ToList();
            }
        }

        /// <summary>
        /// Registers a service with the config system. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bindingName"></param>
        /// <param name="serviceHostName">If empty string (default) it picks the host name form the current config file. if this is not known use 'null'</param>
        /// <returns></returns>
        public static bool RegisterServiceEndpoint<T>(string bindingName, string serviceHostName = "")
        {
            try
            {
                if (Utilities.Utilities.IsNotDevelopementEnvironment())
                    return true;
                EnsurePath();
                var runtime = GetRuntime<T>();
                var interfaceType = GetAndValidateInterfaceType<T>(bindingName);
                var serviceName = GetServiceName<T>(interfaceType);
                if (IsServiceRegistered<T>(serviceName))
                    return true;
                var serviceMessage = CreateRegistrationMessage<T>(bindingName, serviceName, runtime);
                serviceMessage.ServiceHost = GetHostName<T>(serviceHostName);

                var result = runtime.Context.TryRegisterService(serviceMessage);
                if (result)
                {
                    File.AppendAllLines(RegistrationCache, new List<string> { serviceName }, Encoding.UTF8);
                }
                return result;
            }
            catch (Exception ex)
            {
                ex.Log();
                return false;
            }
        }

        private static string GetHostName<T>(string serviceHostName)
        {
            return serviceHostName == string.Empty ? Utilities.Utilities.GetHostName() : serviceHostName;
        }

        private static void EnsurePath()
        {
            if (!PathEnsured)
            {
                var file = new FileInfo(RegistrationCache);
                file.Directory.Create();
                PathEnsured = true;
            }
        }

        private static IRuntime GetRuntime<T>()
        {
            return RuntimeFactory.CreateRuntime()
                .InitializeWithConfigSetName(Utilities.Utilities.GetConfigSetName())
                .SetEnvironment(Utilities.Utilities.GetEnvironment());
        }

        private static bool IsServiceRegistered<T>(string serviceName)
        {
            return File.Exists(RegistrationCache) && File.ReadLines(RegistrationCache, Encoding.UTF8).Contains(serviceName);
        }

        private static ServiceRegistrationMessage CreateRegistrationMessage<T>(string bindingName, string serviceName, IRuntime runtime)
        {
            var variables = GetPropertyNames<T>(bindingName);
            var serviceMessage = new ServiceRegistrationMessage
            {
                ServiceName = serviceName,
                DefaultBinding = bindingName,
                DefaultEnvirionmentUrlPath = HostingEnvironment.ApplicationVirtualPath,
                Environment = runtime.Environment,
                ConfigSetId = Utilities.Utilities.GetConfigSetName(),
                Properties = variables
            };
            return serviceMessage;
        }

        private static List<string> GetPropertyNames<T>(string bindingName)
        {
            var variables = new List<string>();
            if (bindingName != "custom") return variables;
            var variableNames = typeof(T).GetAttribute<ServiceParametersAttribute>();
            if (variableNames.IsInstance())
            {
                variables = variableNames.VariableNames.ToList();
            }
            return variables;
        }


        private static string GetServiceName<T>(Type interfaceType)
        {
            var serviceNameAttr = interfaceType.GetAttribute<ServiceNameAttribute>();
            var serviceName = interfaceType.Name;
            if (serviceName.IsInstance())
                serviceName = serviceNameAttr.ServiceName;
            return serviceName;
        }

        private static Type GetAndValidateInterfaceType<T>(string bindingName)
        {
            var interfaceType = typeof(T);
            if (bindingName == "custom") return interfaceType;
            if (!interfaceType.IsInterface)
                throw new InvalidDataContractException(
                    "The generic type is not an interface, and cannot be registered as a service");
            var isService = interfaceType.GetAttribute<ServiceContractAttribute>();
            if (isService.IsNull())
                throw new InvalidDataContractException(
                    "The generic type is not a service contract, and cannot be registered as a service");
            return interfaceType;
        }

        [Serializable]
        public class ServiceRegistrationMessage
        {
            public string ServiceName { get; set; }
            public string DefaultBinding { get; set; }
            public string DefaultEnvirionmentUrlPath { get; set; }
            public string Environment { get; set; }
            public string ConfigSetId { get; set; }
            public List<string> Properties { get; set; }
            public string ServiceHost { get; set; }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceParametersAttribute : Attribute
    {
        public ServiceParametersAttribute(params string[] variableNames)
        {
            VariableNames = variableNames;
        }

        public string[] VariableNames { get; set; }
    }
}
