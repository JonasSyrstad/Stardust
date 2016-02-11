using Stardust.Starterkit.Configuration.Repository;
using Stardust.Wormhole;

namespace Stardust.Starterkit.Configuration.Web
{
    public class MapDefinitions : IMappingDefinition
    {
        public void Register()
        {
            MapFactory.CreateMapRule<ICacheSettings, ICacheSettings>().GetRule().RemoveMapping("Environment");
        }
    }
}