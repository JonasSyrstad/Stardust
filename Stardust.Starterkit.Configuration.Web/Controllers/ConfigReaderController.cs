using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{


    [Authorize]
    public class ConfigReaderController : ApiController
    {
        private IConfigSetTask reader;

        private readonly IUserFacade userFacade;

        private readonly IEnvironmentTasks environmentReader;

        public ConfigReaderController(IConfigSetTask reader, IUserFacade userFacade, IEnvironmentTasks environmentReader)
        {
            this.reader = reader;
            this.userFacade = userFacade;
            this.environmentReader = environmentReader;
        }

        [HttpGet]
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK,
                "http://localhost:9483/api/ConfigReader?id=Version1.Starterkit&env=Dev");
        }
        private static ConcurrentDictionary<string, ConfigCacheItem> Cache = new ConcurrentDictionary<string, ConfigCacheItem>();
        [HttpGet]
        public HttpResponseMessage Get(string id, string env)
        {
            try
            {
                var environmentId = string.Format("{0}-{1}", id, env);
                var environment = environmentReader.GetEnvironment(environmentId);

                ConfigurationSet data = null;
                ConfigCacheItem cachedData;
                if (Cache.TryGetValue(environmentId, out cachedData))
                {
                    if (cachedData.ETag == environment.ETag)
                    {
                        data = cachedData.ConfigSet;
                    }
                }
                if (data == null)
                {
                    data = reader.GetConfigSetData(id, env);
                    if (cachedData != null)
                    {
                        cachedData.ConfigSet = data;
                        cachedData.ETag = environment.ETag;
                    }
                    else
                    {
                        Cache.TryAdd(environmentId, new ConfigCacheItem { ETag = environment.ETag, ConfigSet = data });
                    }
                }
                data.RequestedBy = User.Identity.Name;
                var result = Request.CreateResponse(HttpStatusCode.OK, data);
                result.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
                return result;
            }
            catch (Exception ex)
            {
                ex.Log();
                throw;
            }

        }


    }

    internal class ConfigCacheItem
    {
        public string ETag { get; set; }

        public ConfigurationSet ConfigSet { get; set; }
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
                var user = userFacade.GetUser(id);
                return Request.CreateResponse(user != null ? new
                                                                 {
                                                                     user.NameId,
                                                                     user.AccessToken,
                                                                     ConfigSets = user.AdministratorType == AdministratorTypes.SystemAdmin ? reader.GetAllConfigSetNames() : user.ConfigSet.Select(c => c.Id).ToList()
                                                                 } : CreateDeletedResponse(id));
            }
            catch (NullReferenceException)
            {
                return Request.CreateResponse(CreateDeletedResponse(id));
            }
        }

        private static object CreateDeletedResponse(string id)
        {
            return new { NameId = id, AccessToken = "deleted", ConfigSets = new List<string>() };
        }
    }
}