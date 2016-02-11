using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BrightstarDB.EntityFramework;
using Stardust.Interstellar.ConfigurationReader;

namespace Stardust.Starterkit.Configuration.Repository
{
    [Entity("ServiceHostSettings")]
    public interface IServiceHostSettings
    {
        [Identifier("http://stardust.com/configuration/ServiceHostSettings", KeyProperties = new[] { "ConfigSetNameId", "Name" }, KeySeparator = "-")]
        string Id { get; }

        [RegularExpression(Constants.KeyValidator, ErrorMessage = "special characters are not  allowed.")]
        string Name { get; set; }

        IConfigSet ConfigSet { get; set; }

        string ConfigSetNameId { get; set; }
        IServiceHostSettings Parent { get; set; }

        [InverseProperty("Parent")]
        ICollection<IServiceHostSettings> Children { get; set; }

        [InverseProperty("ServiceHost")]
        ICollection<IServiceHostParameter> Parameters { get; set; }

        ServiceConfig GetRawConfigData(string environment);

        ICollection<IServiceDescription> Services { get; set; }

        [InverseProperty("DownstreamHosts")]
        ICollection<IServiceHostSettings> UpstreamHosts { get; set; }

        ICollection<IServiceHostSettings> DownstreamHosts { get; set; }

        string Description { get; set; }

        string Visualization { get; set; }

        int Level { get; set; }

        string Alias { get; set; }

        [Ignore]
        string GetDisplayName { get;  }
    }
}