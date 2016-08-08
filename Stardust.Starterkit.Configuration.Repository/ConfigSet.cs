using System;
using System.Collections.Generic;
using System.Linq;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;

namespace Stardust.Starterkit.Configuration.Repository
{
    public partial class ConfigSet
    {
        public override string ToString()
        {
            return Id;
        }

        public ConfigurationSet GetRawConfigData(string environment, out bool doSave)
        {
            var env = Environments.Single(e => e.Name.Equals(environment, StringComparison.OrdinalIgnoreCase));
            doSave = env.ETag.IsNullOrWhiteSpace();
            return new ConfigurationSet
            {
                Created = Created,

                LastUpdated = LastUpdate,
                Environments = GetEnvironmentConfigs(environment),
                SetName = Id,
                ParentSet = ParentConfigSet.IsInstance() ? ParentConfigSet.Id : "",
                Endpoints = (from s in Services select s.GetRawConfigData(environment)).ToList(),
                Services = (from s in ServiceHosts select s.GetRawConfigData(environment)).ToList(),
                ReaderKey = ReaderKey,
                AllowMasterKeyAccess = AllowAccessWithRootKey,
                AllowUserToken = AllowAccessWithUserTokens,
                ETag = env.ETag.ContainsCharacters() ? env.ETag : DateTimeOffset.UtcNow.Ticks.ToString(),
                Version = $"{env.ConfigSet.Version}"
            };
        }

        private List<EnvironmentConfig> GetEnvironmentConfigs(string environment)
        {
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.IncludeAllEnvironments") == "true")
                (from e in Environments select e.GetRawConfigData()).ToList();
            return (from e in Environments where e.Name.Equals(environment, StringComparison.OrdinalIgnoreCase) select e.GetRawConfigData()).ToList();
        }

        public void SetReaderKey(string key)
        {
            ReaderKey = key.Encrypt(KeyHelper.SharedSecret);
        }

        public string GetReaderKey()
        {
            return ReaderKey.Decrypt(KeyHelper.SharedSecret);
        }
    }
}