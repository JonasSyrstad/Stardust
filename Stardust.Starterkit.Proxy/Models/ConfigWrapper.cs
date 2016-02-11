using Stardust.Interstellar.ConfigurationReader;

namespace Stardust.Starterkit.Proxy.Models
{
    public class ConfigWrapper
    {
        public string Environment { get; set; }

        public ConfigurationSet Set { get; set; }

        public string Id { get; set; }
    }
}
