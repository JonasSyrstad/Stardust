using System;
using System.Collections.Generic;
using System.Linq;
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

        private IEnvironmentTasks environmentTasks;

        private IConfigSetTask reader;

        private readonly IUserFacade userFacade;

        public ConfigReaderController(IConfigSetTask reader, IUserFacade userFacade)
        {
            this.reader = reader;
            this.userFacade = userFacade;
        }

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

    [Authorize]
    public class UserTokenController : ApiController
    {
        private IConfigSetTask reader;

        private IUserFacade userFacade;

        public UserTokenController(IConfigSetTask reader, IUserFacade userFacade)
        {
            this.reader = reader;
            this.userFacade = userFacade;
        }
        [HttpGet]
        public HttpResponseMessage GetUser(string id)
        {
            try
            {
                var user = userFacade.GetUser(id.Replace("-", ".").Replace("_", "@"));
                return Request.CreateResponse(user != null ? new { user.NameId, user.AccessToken, ConfigSets = user.ConfigSet.Select(c => c.Id).ToList() } : CreateDeletedResponse(id));
            }
            catch (NullReferenceException)
            {
                return Request.CreateResponse(CreateDeletedResponse(id));
            }
        }

        private static object CreateDeletedResponse(string id)
        {
            return new { NameId=id, AccessToken="deleted", ConfigSets = new List<string>() };
        }
    }
}