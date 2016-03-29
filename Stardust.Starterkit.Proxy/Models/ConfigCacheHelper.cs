using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Stardust.Core.Security;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;

namespace Stardust.Starterkit.Proxy.Models
{
    public static class ConfigCacheHelper
    {
        private static object triowing=new object();
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
            SetCredentials(req);
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

        public static void SetCredentials(HttpWebRequest req)
        {
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.useAzureAd") != "true")
            {
                req.Credentials = new NetworkCredential(
                    ConfigurationManagerHelper.GetValueOnKey("stardust.configUser"),
                    ConfigurationManagerHelper.GetValueOnKey("stardust.configPassword"),
                    ConfigurationManagerHelper.GetValueOnKey("stardust.configDomain"));
            }
            else
            {
                var authContext = new AuthenticationContext(string.Format(ConfigurationManagerHelper.GetValueOnKey("ida:AADInstance"), ConfigurationManagerHelper.GetValueOnKey("ida:Tenant")));

                var clientId = ConfigurationManagerHelper.GetValueOnKey("ida:ClientId");
                var apiResourceId = ConfigurationManagerHelper.GetValueOnKey("ida:ApiResourceId");
                var authResult = authContext.AcquireToken(apiResourceId,
                    new ClientCredential(ConfigurationManagerHelper.GetValueOnKey("stardust.configUser"), 
                        ConfigurationManagerHelper.GetValueOnKey("stardust.configPassword")));

                var bearerToken = authResult.CreateAuthorizationHeader();
                req.Headers.Add("Authorization", bearerToken);
            }
        }

        public static void SetCredentials(HubConnection req)
        {
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.useAzureAd") != "true")
            {
                req.Credentials = new NetworkCredential(
                    ConfigurationManagerHelper.GetValueOnKey("stardust.configUser"),
                    ConfigurationManagerHelper.GetValueOnKey("stardust.configPassword"),
                    ConfigurationManagerHelper.GetValueOnKey("stardust.configDomain"));
            }
            else
            {
                var authContext = new AuthenticationContext(string.Format(ConfigurationManagerHelper.GetValueOnKey("ida:AADInstance"), ConfigurationManagerHelper.GetValueOnKey("ida:Tenant")));

                var clientId = ConfigurationManagerHelper.GetValueOnKey("ida:ClientId");
                var apiResourceId = ConfigurationManagerHelper.GetValueOnKey("ida:ApiResourceId");
                var authResult = authContext.AcquireToken(apiResourceId,
                    new ClientCredential(ConfigurationManagerHelper.GetValueOnKey("stardust.configUser"),
                        ConfigurationManagerHelper.GetValueOnKey("stardust.configPassword")));

                var bearerToken = authResult.CreateAuthorizationHeader();
                req.Headers.Add("Authorization", bearerToken);
            }
        }

        private static ConcurrentDictionary<string, ConfigWrapper> cache = new ConcurrentDictionary<string, ConfigWrapper>();

        internal static ConsolidatedConfigWrapperFile consolidatedWrapper;

        public static ConfigWrapper GetConfigFromCache(string id, string env, string localFile)
        {
            if(UseDiscreteFiles)
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
            else
            {
                lock(triowing)
                {
                   GetOrCreateConsolidatedFile();
                    ConfigWrapper cs;
                    if (consolidatedWrapper.ConfigWrappers.TryGetValue(GetSetId(new ConfigWrapper { Id = id, Environment = env }), out cs)) return cs;
                    return null;
                }
            }
        }

        public static void ValidateToken(this ConfigurationSet configData, string environment, string token, string keyName)
        {
            if (ValidateMasterToken(configData, environment, token, keyName)) return;
            var env = configData.Environments.SingleOrDefault(e => e.EnvironmentName.Equals(environment, StringComparison.OrdinalIgnoreCase));
            Logging.DebugMessage("Validating environment token");
            if (env == null || env.ReaderKey.Decrypt(Secret) != token || !string.Equals(string.Format("{0}-{1}", configData.SetName, env.EnvironmentName), keyName, StringComparison.OrdinalIgnoreCase))
            {

                if (configData.AllowUserToken && UserValidator.ValidateToken(keyName, token, configData.SetName))
                {
                    Logging.DebugMessage("Access to {0}-{1} was granted by user token validation", EventLogEntryType.SuccessAudit, configData.SetName, env);
                    return;
                }
                Logging.DebugMessage("Access to  {0}-{1} was not granted by token validation", EventLogEntryType.FailureAudit, configData.SetName, env.EnvironmentName);
                throw new InvalidDataException("Invalid access token");
            }
            Logging.DebugMessage("Access to {0}-{1} was granted by environment token validation", EventLogEntryType.SuccessAudit, configData.SetName, env);
        }

        private static bool ValidateMasterToken(ConfigurationSet configData, string environment, string token, string keyName)
        {
            if (configData.AllowMasterKeyAccess)
            {
                Logging.DebugMessage("Validating master token");
                try
                {
                    if (token == configData.ReaderKey.Decrypt(Secret) && string.Equals(keyName, configData.SetName, StringComparison.OrdinalIgnoreCase))
                    {
                        Logging.DebugMessage("Access to {0}-{1} was granted by master token validation", EventLogEntryType.SuccessAudit, configData.SetName, environment);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    ex.Log("unable to validate token");
                }
            }
            else if (string.Equals(keyName, configData.SetName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Invalid access token");
            }
            return false;
        }

        public static bool TryValidateToken(this ConfigWrapper config, string env, string token, string keyName = null)
        {
            return config.Set.TryValidateToken(env, token, keyName);
        }

        public static bool TryValidateToken(this ConfigurationSet config, string env, string token, string keyName = null)
        {
            try
            {
                config.ValidateToken(env, token, keyName);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                ex.Log();
                return false;
            }
        }

        public static string GetPathFormat()
        {
            var pathFormat = ConfigurationManagerHelper.GetValueOnKey("stardust.FilePathFormat");
            if (pathFormat.IsNullOrWhiteSpace()) return GetDirectory()+ "\\{0}_{1}.json";
            return String.Format("{0}{1}{2}", AppDomain.CurrentDomain.BaseDirectory + "\\App_Data", (pathFormat.StartsWith("\\") ? "" : "\\"), pathFormat);
        }

        private static string GetDirectory()
        {
            var dirPath= AppDomain.CurrentDomain.BaseDirectory + "\\App_Data" +"\\"+Environment.MachineName;
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            return dirPath ;
        }

        public static string GetLocalFileName(string id, string env)
        {
            return string.Format(GetPathFormat(), id, env);
        }

        private static string GetFileData(string localFile)
        {
            return EncryptFiles() ? File.ReadAllText(localFile).Decrypt(LocalEncryptionKey) : File.ReadAllText(localFile);
        }

        internal static bool EncryptFiles()
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
            key = "defaultEncryptionKey";
            ConfigurationManagerHelper.SetValueOnKey("stardust.ConfigKey", key, true);
            return key;
        }

        public static bool UseDiscreteFiles
        {
            get
            {
                return string.Equals(ConfigurationManagerHelper.GetValueOnKey("stardust.useConsolidatedFile"), "false", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static void UpdateCache(string localFile, ConfigurationSet newConfigSet, ConfigWrapper cs)
        {
            if (UseDiscreteFiles)
            {
                localFile = localFile.Replace("\\\\", "\\").ToLowerInvariant();
                var config = new ConfigWrapper { Set = newConfigSet, Environment = cs.Environment, Id = cs.Id };
                ConfigWrapper oldConfig;
                if (!cache.TryGetValue(localFile, out oldConfig)) cache.TryAdd(localFile, config);
                else
                {
                    if (long.Parse(oldConfig.Set.ETag) <= long.Parse(newConfigSet.ETag)) cache.TryUpdate(localFile, config, oldConfig);
                }
                File.WriteAllText(localFile, GetFileContent(config));
            }
            else
            {
                lock (triowing)
                {
                    GetOrCreateConsolidatedFile();
                    var config = new ConfigWrapper { Set = newConfigSet, Environment = cs.Environment, Id = cs.Id };
                    if (consolidatedWrapper.ConfigWrappers.ContainsKey(GetSetId(cs))) consolidatedWrapper.ConfigWrappers.Remove(GetSetId(cs));
                    consolidatedWrapper.ConfigWrappers.Add(GetSetId(cs), config);
                    
                }
                SaveConsolidatedFile();
            }
        }

        internal static void GetOrCreateConsolidatedFile()
        {
           
            if (consolidatedWrapper == null)
            {
                if (File.Exists(GetConsolidatedFileName()))
                {
                    var content = File.ReadAllText(GetConsolidatedFileName());
                    if (EncryptFiles())
                    {
                        content = content.Decrypt(LocalEncryptionKey);
                    }
                    consolidatedWrapper = JsonConvert.DeserializeObject<ConsolidatedConfigWrapperFile>(content);
                }
                else
                {
                    consolidatedWrapper = new ConsolidatedConfigWrapperFile { ConfigWrappers = new Dictionary<string, ConfigWrapper>(), Users = new Dictionary<string, User>() };
                }
            }
        }

        private static string GetSetId(ConfigWrapper cs)
        {
            return string.Format("{0}-{1}",cs.Id,cs.Environment);
        }

        public static EncryptionKeyContainer LocalEncryptionKey
        {
            get
            {
                var key = ConfigurationManagerHelper.GetValueOnKey("stardust.EncryptionKey");
                if (key.IsNullOrWhiteSpace()) key = Utilities.GetServiceName()+Environment.MachineName;
                return new EncryptionKeyContainer(key);
            }
        }

        private static string GetConsolidatedFileName()
        {
            return GetLocalFileName("consolidated","config");
        }

        private static string GetFileContent(ConfigWrapper config)
        {
            return EncryptFiles() ? JsonConvert.SerializeObject(config).Encrypt(LocalEncryptionKey) : JsonConvert.SerializeObject(config);
        }

        public static void SaveConsolidatedFile()
        {
            lock (triowing)
            {
                var filename = GetConsolidatedFileName();
                var content = JsonConvert.SerializeObject(consolidatedWrapper);
                if (EncryptFiles()) content = content.Encrypt(LocalEncryptionKey);
                File.WriteAllText(filename,content);
            }

        }
    }
}