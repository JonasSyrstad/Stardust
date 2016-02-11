using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Stardust.Clusters;
using Stardust.Core.Security;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;

namespace Stardust.Starterkit.Configuration.Web.Proxy.Controllers
{
    public class ConfigReaderController : ApiController
    {
        private string GetPathFormat()
        {
            var pathFormat = ConfigurationManagerHelper.GetValueOnKey("FilePathFormat");
            if (pathFormat.IsNullOrWhiteSpace()) return AppDomain.CurrentDomain.BaseDirectory + "\\App_Data" + "\\{0}_{1}.json";
            return string.Format("{0}{1}{2}", AppDomain.CurrentDomain.BaseDirectory + "\\App_Data", (pathFormat.StartsWith("\\") ? "" : "\\"), pathFormat);
        }

        [HttpGet]
        [Route("api/ConfigReader/{id}")]
        public HttpResponseMessage Get(string id, string env, string updKey)
        {
            var localFile = string.Format(GetPathFormat(), id, env);
            if (File.Exists(localFile))
            {
                try
                {
                    var configData = ConfigCacheHelper.GetConfigFromCache(localFile).Set;
                    ValidateAccess(configData);
                    return Request.CreateResponse(HttpStatusCode.OK, configData);
                }
                catch (Exception ex)
                {
                    ex.Log();
                }
            }
            return GetAndWriteLocal(id, env, localFile);
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
        private void ValidateAccess(ConfigurationSet configData)
        {
            var cred = EncodingFactory.ReadFileText(Convert.FromBase64String(Request.Headers.Authorization.Parameter));
            var separator = cred.IndexOf(':');
            var name = cred.Substring(0, separator);
            var password = cred.Substring(separator + 1);
            var nameParts = name.Split('\\');
            if (nameParts.Length == 1)
            {
                if (configData.SetName.Equals(nameParts[0], StringComparison.OrdinalIgnoreCase) && configData.ReaderKey.Decrypt(Secret) == password)
                    return;
            }
            else if (nameParts.Length == 2)
            {
                if (configData.SetName.Equals(nameParts[0], StringComparison.OrdinalIgnoreCase))
                {
                    var env = configData.Environments.SingleOrDefault(e => e.EnvironmentName.Equals(nameParts[1], StringComparison.OrdinalIgnoreCase));
                    if (env != null && env.ReaderKey.Decrypt(Secret) == password)
                    {
                        return;
                    }
                }
            }
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        private HttpResponseMessage GetAndWriteLocal(string id, string env, string localFile)
        {

            try
            {
                ConfigurationSet configData;
                configData = GetConfiguration(id, env, localFile);
                ValidateAccess(configData);
                return Request.CreateResponse(HttpStatusCode.OK, configData);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex);
            }
            catch (WebException webEx)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, webEx);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        internal static ConfigurationSet GetConfiguration(string id, string env, string localFile, bool skipSave=false)
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
                if(!skipSave)
                    ConfigCacheHelper.UpdateCache(localFile, configData, new ConfigWrapper { Set = configData, Environment = env, Id = id });
            }
            return configData;
        }

        private static string CreateRequestUriString(string id, string env)
        {
            return string.Format("{0}/api/ConfigReader/{1}?env={2}&updKey{3}", Utilities.GetConfigLocation(), id, env, DateTime.UtcNow.Ticks);
        }
    }
}
