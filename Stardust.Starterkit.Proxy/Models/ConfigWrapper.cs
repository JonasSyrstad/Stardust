using System.Collections.Generic;
using Stardust.Interstellar.ConfigurationReader;

namespace Stardust.Starterkit.Proxy.Models
{
    public class ConfigWrapper
    {
        public string Environment { get; set; }

        public ConfigurationSet Set { get; set; }

        public string Id { get; set; }
    }

    public class ConsolidatedConfigWrapperFile
    {
        public Dictionary<string, ConfigWrapper> ConfigWrappers { get; set; }

        public Dictionary<string, User> Users { get; set; } 
    }
}
