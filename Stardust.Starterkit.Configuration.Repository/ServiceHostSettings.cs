using System;
using System.Linq;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;

namespace Stardust.Starterkit.Configuration.Repository
{
    public partial class ServiceHostSettings
    {
        public override string ToString()
        {
            return Id;
        }

        public ServiceConfig GetRawConfigData(string environment)
        {
            return new ServiceConfig
            {
                ServiceName = Name,
                Parameters = (from p in Parameters select p.GetRawConfigData(environment)).ToList(),
                IdentitySettings = ConfigSet.Environments.Single(x=> string.Equals(x.Name, environment, StringComparison.OrdinalIgnoreCase)).GetRawIdentityData()
            };
        }

        public string GetDisplayName
        {
            get
            {
                return Alias.ContainsCharacters() ? Alias : Name;
            }
        }
    }
}