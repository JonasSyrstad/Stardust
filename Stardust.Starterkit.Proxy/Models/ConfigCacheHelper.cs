using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Stardust.Core.Security;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;

namespace Stardust.Starterkit.Proxy.Models
{
    public static class ConfigCacheHelper
    {
        private static string CreateRequestUriString(string id, string env)
        {
            return String.Format("{0}/api/ConfigReader/{1}?env={2}&updKey{3}", Utilities.GetConfigLocation(), id, env, DateTime.UtcNow.Ticks);
        }

        internal static ConfigurationSet GetConfiguration(string id, string env, string localFile, bool skipSave = false)
        {
            ConfigurationSet configData;
            var req = WebRequest.Create(CreateRequestUriString(id, env)) as HttpWebRequest;
            req.Method = "GET";
            req.Accept = "application/json";
            req.ContentType = "application/json";
            req.Headers.Add("Accept-Language", "en-us");
            req.UserAgent = "StardustProxy/1.0";
            req.Credentials = new NetworkCredential(
                ConfigurationManagerHelper.GetValueOnKey("stardust.configUser"),
                ConfigurationManagerHelper.GetValueOnKey("stardust.configPassword"),
                ConfigurationManagerHelper.GetValueOnKey("stardust.configDomain"));
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            var resp = req.GetResponse();

            using (var reader = new StreamReader(resp.GetResponseStream()))
            {
                configData = JsonConvert.DeserializeObject<ConfigurationSet>(reader.ReadToEnd());
                if (!skipSave)
                    UpdateCache(localFile, configData, new ConfigWrapper { Set = configData, Environment = env, Id = id });
            }
            return configData;
        }
        private static ConcurrentDictionary<string, ConfigWrapper> cache = new ConcurrentDictionary<string, ConfigWrapper>();

        public static ConfigWrapper GetConfigFromCache(string localFile)
        {
            localFile = localFile.Replace("\\\\", "\\").ToLowerInvariant();
            ConfigWrapper config;
            if (!cache.TryGetValue(localFile, out config))
            {
                config = JsonConvert.DeserializeObject<ConfigWrapper>(GetFileData(localFile));
                cache.TryAdd(localFile, config);
            }
            return config;
        }

        public static string GetPathFormat()
        {
            var pathFormat = ConfigurationManagerHelper.GetValueOnKey("FilePathFormat");
            if (pathFormat.IsNullOrWhiteSpace()) return AppDomain.CurrentDomain.BaseDirectory + "\\App_Data" + "\\{0}_{1}.json";
            return String.Format("{0}{1}{2}", AppDomain.CurrentDomain.BaseDirectory + "\\App_Data", (pathFormat.StartsWith("\\") ? "" : "\\"), pathFormat);
        }

        public static string GetLocalFileName(string id, string env)
        {
            return String.Format(GetPathFormat(), id, env);
        }

        private static string GetFileData(string localFile)
        {
            return EncryptFiles() ? File.ReadAllText(localFile).Decrypt(Secret) : File.ReadAllText(localFile);
        }

        private static bool EncryptFiles()
        {
            return ConfigurationManagerHelper.GetValueOnKey("stardust.EncryptCacheFiles") == "true";
        }

        internal static EncryptionKeyContainer Secret
        {
            get
            {
                return new EncryptionKeyContainer(GetKeyFromConfig());
            }
        }

        private static string GetKeyFromConfig()
        {
            var key = ConfigurationManagerHelper.GetValueOnKey("stardust.ConfigKey");
            if (key.ContainsCharacters()) return key;
            key = "mayTheKeysSupportAllMyValues";
            ConfigurationManagerHelper.SetValueOnKey("stardust.ConfigKey", key, true);
            return key;
        }

        public static void UpdateCache(string localFile, ConfigurationSet newConfigSet, ConfigWrapper cs)
        {
            localFile = localFile.Replace("\\\\", "\\").ToLowerInvariant();
            var config = new ConfigWrapper { Set = newConfigSet, Environment = cs.Environment, Id = cs.Id };
            ConfigWrapper oldConfig;
            if (!cache.TryGetValue(localFile, out oldConfig)) cache.TryAdd(localFile, config);
            else
            {
                if (long.Parse(oldConfig.Set.ETag) <= long.Parse(newConfigSet.ETag))
                    cache.TryUpdate(localFile, config, oldConfig);
            }
            File.WriteAllText(localFile, GetFileContent(config));
        }

        private static string GetFileContent(ConfigWrapper config)
        {
            return EncryptFiles() ? JsonConvert.SerializeObject(config).Encrypt(Secret) : JsonConvert.SerializeObject(config);
        }
    }
}