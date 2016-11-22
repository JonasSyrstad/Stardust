using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BrightstarDB.EntityFramework;
using Stardust.Interstellar.ConfigurationReader;

namespace Stardust.Starterkit.Configuration.Repository
{
    [Entity("ServiceHostParameter")]
    public interface IServiceHostParameter
    {
        [Identifier("http://stardust.com/configuration/ServiceHost/Parameters/", KeyProperties = new[] { "ServiceHostSettingsNameId", "Name" }, KeySeparator = "-")]
        string Id { get; }
        [RegularExpression(Constants.KeyValidator, ErrorMessage = "special characters are not  allowed.")]
        string Name { get; set; }

        string Description { get; set; }

        string ServiceHostSettingsNameId { get; set; }

        bool IsSecureString { get; set; }

        IServiceHostSettings ServiceHost { get; set; }

        IServiceHostParameter Parent { get; set; }

        [InverseProperty("Parent")]
        ICollection<IServiceHostParameter> ChildParameters { get; set; }

        string ItemValue { get; set; }

        [Ignore]
        string Value { get; }

        [Ignore]
        bool IsRoot { get; }
        [Ignore]
        bool IsInherited { get; }

        ConfigParameter GetRawConfigData(string environment);

        void SetValue(string value);

        byte[] BinaryValue { get; set; }

        bool IsEnvironmental { get; set; }

        [Ignore]
        Dictionary<string, string> EnvironmentalValue { get; }

        [InverseProperty("HostParameters")]
        ICollection<ISubstitutionParameter> SubstitutionParameters { get; set; }

        [InverseProperty("HostParameters")]
        ICollection<IEndpointParameter> EndpointParameters { get; set; }
        
    }
}