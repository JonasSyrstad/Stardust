using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public static void ValidateToken(this ConfigurationSet configData,  string environment, string token,string keyName)
        {
            if (ValidateMasterToken(configData, environment, token, keyName)) return;
            var env = configData.Environments.SingleOrDefault(e => e.EnvironmentName.Equals(environment, StringComparison.OrdinalIgnoreCase));

            if (env == null || env.ReaderKey.Decrypt(Secret) != token || !string.Equals(string.Format("{0}-{1}", configData.SetName, env.EnvironmentName), keyName, StringComparison.OrdinalIgnoreCase))
            {
                if (UserValidator.ValidateToken(keyName, token, configData.SetName))
                {
                    Logging.DebugMessage("Access to {0}-{1} was granted by token validation", EventLogEntryType.SuccessAudit, configData.SetName, env);
                    return;
                }
                Logging.DebugMessage("Access to  {0}-{1} was not granted by token validation", EventLogEntryType.FailureAudit, configData.SetName, env.EnvironmentName);
                throw new InvalidDataException("Invalid access token");
            }
            Logging.DebugMessage("Access to {0}-{1} was granted by token validation", EventLogEntryType.SuccessAudit, configData.SetName, env);
        }

        private static bool ValidateMasterToken(ConfigurationSet configData, string environment, string token, string keyName)
        {
            if (configData.AllowMasterKeyAccess)
            {
                try
                {
                    if (token == configData.ReaderKey.Decrypt(Secret) && string.Equals(keyName, configData.SetName, StringComparison.OrdinalIgnoreCase))
                    {
                        Logging.DebugMessage("Access to {0}-{1} was granted by token validation", EventLogEntryType.SuccessAudit, configData.SetName, environment);
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

        public static bool TryValidateToken(this ConfigWrapper config, string env, string token,string keyName=null)
        {
            return config.Set.TryValidateToken(env, token,keyName);
        }

        public static bool TryValidateToken(this ConfigurationSet config, string env, string token, string keyName=null)
        {
            try
            {
                config.ValidateToken(env, token,keyName);
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

    public static  class UserValidator
    {

        private static ConcurrentDictionary<string,User> userCache=new ConcurrentDictionary<string, User>();
        public static bool ValidateToken(string username, string token, string configSet)
        {
            var user = LoadUser(username);
            if (user != null)
            {
                if (!string.Equals(user.NameId, username, StringComparison.OrdinalIgnoreCase) || user.AccessToken.Decrypt(ConfigCacheHelper.Secret) != token) return false;
            }
            return true;
        }

        private static User LoadUser(string username)
        {
            User user;
            if(userCache.TryGetValue(username,out user)) return user;
            lock (userCache)
            {
                if (userCache.TryGetValue(username, out user)) return user;
                var fileName = ConfigCacheHelper.GetLocalFileName(username, "user");
                if (File.Exists(fileName))
                {
                    var fileContent = File.ReadAllText(fileName);
                    if (ConfigCacheHelper.EncryptFiles()) fileContent = fileContent.Decrypt(ConfigCacheHelper.Secret);
                    user = JsonConvert.DeserializeObject<User>(fileContent);
                    userCache.TryAdd(username, user);
                    return user;
                }
                user = GetConfiguration(username);
                userCache.TryAdd(username, user);
                return user; 
            }

        }

        internal static User GetConfiguration(string username)
        {
            User configData;
            var req = WebRequest.Create(CreateRequestUriString(username)) as HttpWebRequest;
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
                configData = JsonConvert.DeserializeObject<User>(reader.ReadToEnd());
            }
            return configData;
        }

        private static string CreateRequestUriString(string username)
        {
            return string.Format("{0}/api/UserToken/{1}?updid{2}", Utilities.GetConfigLocation(), username, DateTime.UtcNow.Ticks);
        }

        public static void UpdateUser(string username)
        {
            var user = GetConfiguration(username);
            lock (userCache)
            {
                User oldUser;
                userCache.TryRemove(username, out oldUser);
                userCache.TryAdd(username, user);
                var fileContent = JsonConvert.SerializeObject(user);
                if (ConfigCacheHelper.EncryptFiles()) fileContent = fileContent.Encrypt(ConfigCacheHelper.Secret);
                File.WriteAllText(ConfigCacheHelper.GetLocalFileName(username, "user"),fileContent);
            }
        }
    }

    class User
    {
        public string NameId { get; set; }
        public string AccessToken { get; set; }
        public List<string> ConfigSets { get; set; }
    }
}