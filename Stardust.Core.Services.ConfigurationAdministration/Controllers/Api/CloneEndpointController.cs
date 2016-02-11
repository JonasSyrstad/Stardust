using System.Web.Http;
using Stardust.Core.FactoryHelpers;
using Stardust.Core.Services.ConfigurationReader;

namespace Stardust.Core.Services.ConfigurationAdministration.Controllers.Api
{
    public class CloneEndpointController : ApiController
    {
        [HttpGet]
        public bool Clone(string id, int eid, string name)
        {
            try
            {
                var item = GetConfigurationReader().GetConfiguration(id);
                var ep = (EndpointConfig)GetEndpoint(item, id, eid).Clone();
                ep.ServiceName = name;
                ep.Id = item.Endpoints.Count;
                item.Endpoints.Add(ep);
                GetConfigurationReader().WriteConfigurationSet(item, id);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static IConfigurationReader GetConfigurationReader()
        {
            var reader = Resolver.Resolve<IConfigurationReader>().Activate(Scope.Singleton);
            return reader;
        }

        private static EndpointConfig GetEndpoint(ConfigurationSet item, string id, int eid)
        {
            var endpoint = item.Endpoints[eid];
            return endpoint;
        }
    }
}