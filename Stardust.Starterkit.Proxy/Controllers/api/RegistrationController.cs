using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Stardust.Interstellar;
using Stardust.Particles;
using Stardust.Starterkit.Proxy.Models;

namespace Stardust.Starterkit.Proxy.Controllers.api
{
    [RoutePrefix("api/registration")]
    public class RegistrationController : ConfigReaderControllerBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Web.Mvc.Controller"/> class.
        /// </summary>
        public RegistrationController()
        {
        }

        [System.Web.Http.HttpPost]
        [Route("TryAddService")]
        public HttpResponseMessage TryAddService([FromBody] ServiceRegistrationServer.ServiceRegistrationMessage data)
        {
            var localFile = ConfigCacheHelper.GetLocalFileName(data.ConfigSetId, data.Environment);
            if (File.Exists(localFile))
            {
                var configData = ConfigCacheHelper.GetConfigFromCache(data.ConfigSetId, data.Environment, localFile).Set;
                ValidateAccess(configData, data.Environment);

            }
            return Request.CreateResponse(HttpStatusCode.OK);

        }
    }
}