using BrightstarDB.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stardust.Starterkit.Configuration.Repository
{
    [Entity("Endpoint")]
    public interface IEndpoint
    {
        [Identifier("http://stardust.com/configuration/ServiceDescriptions/Endpoints", KeyProperties = new[] { "ServiceNameId", "Name" }, KeySeparator = "-")]
        string Id { get; }

          [RegularExpression(Constants.KeyValidator, ErrorMessage = "special characters are not  allowed.")]
        string Name { get; set; }

        string ServiceNameId { get; set; }

        [InverseProperty("ParentEndpoint")]
        ICollection<IEndpoint> ChildEndpoints { get; set; }

        IEndpoint ParentEndpoint { get; set; }

        IServiceDescription ServiceDescription { get; set; }

        [InverseProperty("Endpoint")]
        ICollection<IEndpointParameter> Parameters { get; set; }

        Interstellar.ConfigurationReader.Endpoint GetRawConfigData(string environment);
        void SetFromRawData(Interstellar.ConfigurationReader.Endpoint endpoint, ConfigurationContext repository);
    }
}
