using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;

namespace Stardust.Core.Default.Implementations
{
    public class LocalFileConfigReader:IConfigurationReader
    {
        /// <summary>
        /// Reads the configSet from the store
        /// </summary>
        /// <param name="setName"/><param name="environment"/>
        /// <returns/>
        public ConfigurationSet GetConfiguration(string setName, string environment = null)
        {
            if (environment.IsNullOrWhiteSpace()) environment = Utilities.GetEnvironment();
            var path = string.Format(GetPathFormat(), setName, environment);
            var data = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ConfigurationSet>(data);
        }

        private string GetPathFormat()
        {
            var pathFormat = ConfigurationManagerHelper.GetValueOnKey("FilePathFormat");
            if (pathFormat.IsNullOrWhiteSpace()) return AppDomain.CurrentDomain.BaseDirectory + "\\{0}_{1}.json";
            return string.Format("{0}{1}{2}", AppDomain.CurrentDomain.BaseDirectory, (pathFormat.StartsWith("\\")?"":"\\"), pathFormat);
        }

        /// <summary>
        /// Not implemented in any of the defaults. Not hooked up to the framework, may add support for this at a later stage.
        /// </summary>
        /// <param name="configuration"/><param name="setName"/>
        public void WriteConfigurationSet(ConfigurationSet configuration, string setName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the original configuration data received from the config service
        /// </summary>
        /// <param name="setName"/>
        /// <returns/>
        public T GetRawConfigData<T>(string setName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the original configuration data received from the config service
        /// </summary>
        /// <param name="setName"/>
        /// <returns/>
        public object GetRawConfigData(string setName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ConfigurationSet> GetAllSets()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tries to register a service with the configuration system.
        /// </summary>
        /// <param name="serviceMessage"/>
        /// <returns/>
        public bool TryRegisterService(ServiceRegistrationServer.ServiceRegistrationMessage serviceMessage)
        {
            return false;
        }

        /// <summary>
        /// This will only be used if the Reader implementation supports change notification
        /// </summary>
        /// <param name="onCacheChanged"/>
        public void SetCacheInvalidatedHandler(Action<ConfigurationSet> onCacheChanged)
        {
        }
    }
}
