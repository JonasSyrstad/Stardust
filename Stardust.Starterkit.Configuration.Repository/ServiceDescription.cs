using System.Linq;
using Stardust.Interstellar.ConfigurationReader;

namespace Stardust.Starterkit.Configuration.Repository
{
    public partial class ServiceDescription
    {
        public override string ToString()
        {
            return Id;
        }

        public string ClientEndpoint
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ClientEndpointValue))
                    return PatentServiceDescription != null ? PatentServiceDescription.ClientEndpoint : "";
                return ClientEndpointValue;
            }
        }

        public EndpointConfig GetRawConfigData(string environment)
        {
            return new EndpointConfig
            {
                ServiceName = Name,
                ActiveEndpoint = ClientEndpoint,
                Endpoints = (from e in Endpoints select e.GetRawConfigData(environment)).ToList()
            };
        }
    }
}