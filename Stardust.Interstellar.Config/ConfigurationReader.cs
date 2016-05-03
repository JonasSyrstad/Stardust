using System;
using System.Configuration;
using System.Linq;
using Stardust.Interstellar.Rest;

namespace Stardust.Interstellar.Config
{
    public static class Configuration
    {
        public static void SetCacheHandler(IConfigReaderCache cacheHandler)
        {
            cacheHandler?.SetReaderProxy(ProxyFactory.CreateInstance<IConfigReader>(ConfigUrl));
            cache = cacheHandler;
        }
        public static void SetApiToken(IConfigReader reader, string token, string tokenKey)
        {
            StardustConfigCredentials.SetApiToken(token, tokenKey);
        }

        public static void SetUsername(string username, string password)
        {
            SetUsername(username, password, null);
        }

        public static void SetUsername(string username, string password, string domain)
        {
            StardustConfigCredentials.SetUsername(username, password, domain);
        }

        public static string ConfigUrl
        {
            get
            {
                if (configUrl == null) configUrl = ConfigurationManager.AppSettings["stardust.configLocation"];
                return configUrl;
            }
            set
            {
                configUrl = value;
            }
        }

        public static EnvironmentConfig CurrentEnvironment
        {
            get
            {
                return GetConfiguration().Environments.Single(e => e.EnvironmentName.Equals(GetEnvironmentName(), StringComparison.OrdinalIgnoreCase));
            }
        }


        public static ServiceConfig CurrentHost
        {
            get
            {
                return GetConfiguration().Services.Single(host => host.ServiceName.Equals(GetServiceHostName()));
            }
        }

        public static string GetConfigSetName()
        {
            if (!string.IsNullOrWhiteSpace(configSet)) return configSet;
            configSet = ConfigurationManager.AppSettings["stardust.configSet"];
            if (!string.IsNullOrWhiteSpace(configSet)) return configSet;
            configSet = ConfigurationManager.AppSettings["configSet"];
            return configSet;
        }

        public static ConfigurationSet GetConfiguration(string setName, string environmentName)
        {
            ConfigurationSet configData;
            if (cache != null)
            {

                if (cache.TryGetConfig(setName, environmentName, out configData)) return configData;


            }
            configData = ProxyFactory.CreateInstance<IConfigReader>(ConfigUrl).Get(setName, environmentName, DateTime.Now.Ticks.ToString());
            if (cache != null) cache.TryAddConfig(setName, environmentName, configData);
            return configData;
        }

        /// <summary>
        /// Retreives the configuration for AppSettings 
        /// stardust.configSet
        /// stardust.environment
        /// </summary>
        /// <returns></returns>
        public static ConfigurationSet GetConfiguration()
        {
            return GetConfiguration(GetConfigSetName(), GetEnvironmentName());
        }

        public static string GetEnvironmentName()
        {
            if (!string.IsNullOrWhiteSpace(serviceName)) return environment;
            serviceName = ConfigurationManager.AppSettings["stardust.environment"];
            if (!string.IsNullOrWhiteSpace(environment)) return serviceName;
            serviceName = ConfigurationManager.AppSettings["environment"];
            return serviceName;
        }

        public static string GetServiceHostName()
        {
            if (!string.IsNullOrWhiteSpace(environment)) return serviceName;
            environment = ConfigurationManager.AppSettings["stardust.environment"];
            if (!string.IsNullOrWhiteSpace(environment)) return environment;
            environment = ConfigurationManager.AppSettings["environment"];
            return environment;
        }
        private static string configSet;
        private static string configUrl;
        private static string environment;

        private static string serviceName;

        private static IConfigReaderCache cache;
    }
}