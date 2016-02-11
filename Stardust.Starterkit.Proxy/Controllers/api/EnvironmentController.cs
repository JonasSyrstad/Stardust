using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;
using Stardust.Starterkit.Proxy.Models;

namespace Stardust.Starterkit.Proxy.Controllers.api
{
    [RoutePrefix("js")]
    public class EnvironmentController : ConfigReaderControllerBase
    {
        public EnvironmentController()
        {
        }

        [Route("{configSet}/{environment}/{key}")]
        [HttpGet, HttpPost]
        public HttpResponseMessage Get(string configSet, string environment, string key)
        {
            var localFile = ConfigCacheHelper.GetLocalFileName(configSet, environment);
            ConfigurationSet configData = null;
            try
            {
                if (!File.Exists(localFile)) configData = ConfigCacheHelper.GetConfiguration(configSet, environment, localFile);
                else configData = ConfigCacheHelper.GetConfigFromCache(localFile).Set;
                ValidateAccess(configData, environment);
            }
            catch (UnauthorizedAccessException ex)
            {
                return CreateUnauthenticatedMessage(ex);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, new HttpError(ex.Message));
            }
            if (configData == null) return Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "Invalid config set" });
            var env = configData.Environments.SingleOrDefault(e => e.EnvironmentName.Equals(environment, StringComparison.OrdinalIgnoreCase));
            if (env == null) return Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "Environment not defined" });
            var param = env.GetConfigParameter(key);
            return Request.CreateResponse(param.ContainsCharacters() ? HttpStatusCode.OK : HttpStatusCode.NoContent, param.ContainsCharacters() ? new { Name = key, Value = param } : null);
        }

        [Route("{configSet}/{environment}/{host}/{key}")]
        [HttpGet, HttpPost]
        public HttpResponseMessage GetHostParameter(string configSet, string environment, string host, string key)
        {
            var localFile = ConfigCacheHelper.GetLocalFileName(configSet, environment);
            ConfigurationSet configData = null;
            try
            {
                if (!File.Exists(localFile)) configData = ConfigCacheHelper.GetConfiguration(configSet, environment, localFile);
                else configData = ConfigCacheHelper.GetConfigFromCache(localFile).Set;
                ValidateAccess(configData, environment);
            }
            catch (UnauthorizedAccessException ex)
            {
                return CreateUnauthenticatedMessage(ex);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, new HttpError(ex.Message));
            }
            if (configData == null) return Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "Invalid config set" });
            var env = configData.Environments.SingleOrDefault(e => e.EnvironmentName.Equals(environment, StringComparison.OrdinalIgnoreCase));
            if (env == null) return Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "Environment not defined" });
            var hostData = configData.Services.SingleOrDefault(h => h.ServiceName.Equals(host, StringComparison.OrdinalIgnoreCase));
            if (hostData == null) return Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "Service host not defined" });
            var param = hostData.GetConfigParameter(key);
            return Request.CreateResponse(param.ContainsCharacters() ? HttpStatusCode.OK : HttpStatusCode.NoContent, param.ContainsCharacters() ? new { Name = key, Value = param } : null);
        }
    }
}
