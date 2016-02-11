using System;
using System.Web.Http;
using Stardust.Core.FactoryHelpers;
using Stardust.Core.Services.ConfigurationReader;

namespace Stardust.Core.Services.ConfigurationAdministration.Controllers.Api
{
    public class CloneController : ApiController
    {
        private static IConfigurationReader GetConfigurationReader()
        {
            var reader = Resolver.Resolve<IConfigurationReader>().Activate(Scope.Singleton);
            return reader;
        }

        [HttpGet]
        public bool Clone(string id)
        {
            var vals = id.Split('|');
            var clonedSet = (ConfigurationSet)GetConfigurationReader().GetConfiguration(vals[0]).Clone();
            clonedSet.ParentSet = clonedSet.SetName;
            clonedSet.SetName = vals[1];
            clonedSet.Created = DateTime.Now; 
            clonedSet.LastUpdated = DateTime.Now;
            GetConfigurationReader().WriteConfigurationSet(clonedSet, clonedSet.SetName);
            return true;
        }
    }
}
