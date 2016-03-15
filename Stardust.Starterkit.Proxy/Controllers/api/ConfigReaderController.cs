using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;
using Stardust.Starterkit.Proxy.Models;

namespace Stardust.Starterkit.Proxy.Controllers.api
{
    public class ConfigReaderController : ConfigReaderControllerBase
    {


        [HttpGet]
        public HttpResponseMessage Get(string id, string env = null, string updKey = null)
        {
            var localFile = ConfigCacheHelper.GetLocalFileName(id, env);
            if (File.Exists(localFile))
            {
                try
                {
                    var configData = ConfigCacheHelper.GetConfigFromCache(id, env, localFile).Set;
                    ValidateAccess(configData, env);
                    return CreateResponse(configData);
                }
                catch (UnauthorizedAccessException ex)
                {
                    return CreateUnauthenticatedMessage(ex);
                }
                catch (Exception ex)
                {
                    ex.Log();
                }
            }
            try
            {
                return CreateResponse(GetAndWriteLocal(id, env, localFile));
            }
            catch (UnauthorizedAccessException ex)
            {
                return CreateUnauthenticatedMessage(ex);
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

        private HttpResponseMessage CreateResponse(ConfigurationSet configData)
        {
            return Request.CreateResponse(HttpStatusCode.OK, PrepareDataForTransmission(configData));
        }

        private static ConfigurationSet PrepareDataForTransmission(ConfigurationSet configData)
        {
            var returnData = JsonConvert.DeserializeObject<ConfigurationSet>(JsonConvert.SerializeObject(configData));
            returnData.ReaderKey = null;
            foreach (var environmentConfig in returnData.Environments)
            {
                environmentConfig.ReaderKey = null;
            }
            returnData.RequestedBy = null;
            return returnData;
        }

        private ConfigurationSet GetAndWriteLocal(string id, string env, string localFile)
        {
            ConfigurationSet configData;
            configData = ConfigCacheHelper.GetConfiguration(id, env, localFile);
            ValidateAccess(configData, env);
            return configData;
        }

        public ConfigReaderController()
        {
        }
    }
}
