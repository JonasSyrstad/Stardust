using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;

namespace Stardust.Starterkit.Proxy.Models
{
    public static class UserValidator
    {

        private static ConcurrentDictionary<string, User> userCache = new ConcurrentDictionary<string, User>();
        public static bool ValidateToken(string username, string token, string configSet)
        {
            Logging.DebugMessage("Validating user token");
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
            if (ConfigCacheHelper.UseDiscreteFiles)
            {
                if (userCache.TryGetValue(username, out user)) return user;
                lock (userCache)
                {
                    if (userCache.TryGetValue(username, out user)) return user;
                    var fileName = ConfigCacheHelper.GetLocalFileName(username, "user");
                    if (File.Exists(fileName))
                    {
                        var fileContent = File.ReadAllText(fileName);
                        if (ConfigCacheHelper.EncryptFiles()) fileContent = fileContent.Decrypt(ConfigCacheHelper.LocalEncryptionKey);
                        user = JsonConvert.DeserializeObject<User>(fileContent);
                        userCache.TryAdd(username, user);
                        return user;
                    }
                    user = GetConfiguration(username);
                    userCache.TryAdd(username, user);
                    var content = JsonConvert.SerializeObject(user);
                    if (ConfigCacheHelper.EncryptFiles()) content = content.Encrypt(ConfigCacheHelper.LocalEncryptionKey);
                    File.WriteAllText(fileName,content);
                    return user;
                }
            }
            else
            {
                lock (userCache)
                {
                    ConfigCacheHelper.GetOrCreateConsolidatedFile();
                    if(ConfigCacheHelper.consolidatedWrapper.Users.TryGetValue(username,out user)) return user;
                    user = GetConfiguration(username);
                    ConfigCacheHelper.consolidatedWrapper.Users.Add(username,user);
                    ConfigCacheHelper.SaveConsolidatedFile();
                    return user;
                }
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
            ConfigCacheHelper.SetCredentials(req);
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
            if (ConfigCacheHelper.UseDiscreteFiles)
            {
                lock (userCache)
                {
                    User oldUser;
                    userCache.TryRemove(username, out oldUser);
                    userCache.TryAdd(username, user);
                    var fileContent = JsonConvert.SerializeObject(user);
                    if (ConfigCacheHelper.EncryptFiles()) fileContent = fileContent.Encrypt(ConfigCacheHelper.LocalEncryptionKey);
                    File.WriteAllText(ConfigCacheHelper.GetLocalFileName(username, "user"), fileContent);
                }
            }
            else
            {
                ConfigCacheHelper.GetOrCreateConsolidatedFile();
                if (ConfigCacheHelper.consolidatedWrapper.Users.ContainsKey(username)) ConfigCacheHelper.consolidatedWrapper.Users.Remove(username);
                ConfigCacheHelper.consolidatedWrapper.Users.Add(username,user);
                ConfigCacheHelper.SaveConsolidatedFile();
            }
        }
    }
}