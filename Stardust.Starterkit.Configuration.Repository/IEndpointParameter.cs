using BrightstarDB.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Stardust.Starterkit.Configuration.Repository
{
    [Entity]
    public interface IEndpointParameter
    {
        [Identifier("http://stardust.com/configuration/ServiceDescriptions/Endpoints/Parameters", KeyProperties = new[] { "EndpointNameId", "Name" }, KeySeparator = "-")]
        string Id { get; }
          [RegularExpression(Constants.KeyValidator, ErrorMessage = "special characters are not  allowed.")]
        string Name { get; set; }

        IEndpoint Endpoint { get; set; }

        string EndpointNameId { get; set; }

        IEndpointParameter Parent { get; set; }

        [InverseProperty("Parent")]
        ICollection<IEndpointParameter> ChildParameters { get; set; }

        string ItemValue { get; set; }

        [DisplayName("Environmental")]
        bool ConfigurableForEachEnvironment { get; set; }

        [Ignore]
        [DisplayName("Inherited value")]
        string Value { get; }

        [Ignore]
        Dictionary<string, string> EnvironmentalValue { get; }

        [Ignore]
        bool IsRoot { get; }

        bool IsPerService { get; set; }
    }
}