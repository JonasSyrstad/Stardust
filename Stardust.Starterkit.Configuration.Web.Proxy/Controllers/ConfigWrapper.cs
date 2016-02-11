using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stardust.Interstellar.ConfigurationReader;

namespace Stardust.Starterkit.Configuration.Web.Proxy.Controllers
{
    public class ConfigWrapper
    {
        public string Environment { get; set; }

        public ConfigurationSet Set { get; set; }

        public string Id { get; set; }
    }
}
