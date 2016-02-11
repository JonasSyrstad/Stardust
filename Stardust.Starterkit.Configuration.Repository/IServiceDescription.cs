using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BrightstarDB.EntityFramework;
using Stardust.Interstellar.ConfigurationReader;

namespace Stardust.Starterkit.Configuration.Repository
{
    [Entity("ServiceDescription")]
    public interface IServiceDescription
    {
        [Identifier("http://stardust.com/configuration/ServiceDescriptions", KeyProperties = new[] { "ConfigSetNameId", "Name" }, KeySeparator = "-")]
        string Id { get; }

        [RegularExpression(Constants.KeyValidator, ErrorMessage = "special characters are not  allowed.")]
        string Name { get; set; }

        [DisplayName("Client endpoint")]
        string ClientEndpointValue { get; set; }

        [Ignore]
        [DisplayName("Active endpoint")]
        string ClientEndpoint { get; }

        IConfigSet ConfigSet { get; set; }

        string ConfigSetNameId { get; set; }

        [InverseProperty("PatentServiceDescription")]
        ICollection<IServiceDescription> ChildServiceDescriptions { get; set; }

        IServiceDescription PatentServiceDescription { get; set; }

        [InverseProperty("ServiceDescription")]
        ICollection<IEndpoint> Endpoints { get; set; }

        EndpointConfig GetRawConfigData(string environment);

        [InverseProperty("Services")]
        IServiceHostSettings ServiceHost { get; set; }

        string ServiceHostId { get; set; }

        string Description { get; set; }
    }
}