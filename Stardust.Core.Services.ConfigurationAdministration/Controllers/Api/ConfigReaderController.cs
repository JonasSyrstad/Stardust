using System.Web.Http;
using Stardust.Core.FactoryHelpers;
using Stardust.Core.Services.ConfigurationReader;

namespace Stardust.Core.Services.ConfigurationAdministration.Controllers.Api
{
    public class ConfigReaderController : ApiController
    {
        private static IConfigurationReader GetConfigurationReader()
        {
            var reader = Resolver.Resolve<IConfigurationReader>().Activate(Scope.Singleton);
            return reader;
        }

        [Authorize]
        public ConfigurationSet Get(string id)
        {
            var set= GetConfigurationReader().GetConfiguration(id);
            set.RequestedBy = User.Identity.Name;
            return set;
        }
    }
}
