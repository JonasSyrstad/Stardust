using Stardust.Starterkit.Configuration.Repository;
using Stardust.Starterkit.Configuration.Web.Models;
using Stardust.Wormhole;

namespace Stardust.Starterkit.Configuration.Web
{
    public class MapDefinitions : IMappingDefinition
    {
        public void Register()
        {
            MapFactory.CreateMapRule<ICacheSettings, ICacheSettings>().GetRule().RemoveMapping("Environment");
            MapFactory.CreateMapRule<IServiceHostParameter, PropertyRequest>().Add(s => s.Name, t => t.PropertyName).Add(s => s.ServiceHost.Name, t => t.ParentContainer);
            MapFactory.CreateMapRule<IEnvironmentParameter, PropertyRequest>().Add(s => s.Name, t => t.PropertyName).Add(s => s.Environment.ConfigSet.Id, t => t.ParentContainer).Add(s => s.Environment.ConfigSet.Id, t => t.SubContainer);
        }
    }
}