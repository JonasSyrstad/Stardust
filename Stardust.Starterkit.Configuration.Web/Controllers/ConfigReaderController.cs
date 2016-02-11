using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Business;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{


    [Authorize]
    public class ConfigReaderController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK,
                "http://localhost:9483/api/ConfigReader?id=Version1.Starterkit&env=Dev");
        }
       
        [HttpGet]
        public HttpResponseMessage Get(string id, string env)
        {
            try
            {
                var reader = ConfigReaderFactory.GetConfigSetTask();
                var data = reader.GetConfigSetData(id, env);
                data.RequestedBy = User.Identity.Name;

                var result = Request.CreateResponse(HttpStatusCode.OK, data);
                result.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
                return result;
            }
            catch (System.Exception ex)
            {
                ex.Log();
                throw;
            }

        }
    }
}