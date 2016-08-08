using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BrightstarDB.EntityFramework;
using Stardust.Interstellar.ConfigurationReader;

namespace Stardust.Starterkit.Configuration.Repository
{
    [Entity("EnvironmentParameter")]
    public interface IEnvironmentParameter
    {
        [Identifier("http://stardust.com/configuration/Environments/Parameters/", KeyProperties = new[] { "EnvironmentNameId", "Name" }, KeySeparator = "-")]
        string Id { get; }
        [RegularExpression(Constants.KeyValidator, ErrorMessage = "special characters are not  allowed.")]
        string Name { get; set; }

        string Description { get; set; }

        string EnvironmentNameId { get; set; }
            
        IEnvironment Environment { get; set; }

        IEnvironmentParameter Parent { get; set; }

        [InverseProperty("Parent")]
        ICollection<IEnvironmentParameter> ChildParameters { get; set; }

        string ItemValue { get; set; }

        [Ignore]
        string Value { get; }

        [Ignore]
        bool IsRoot { get; }
        [Ignore]
        bool IsInherited { get; }

        ConfigParameter GetRawConfigData();

        bool IsSecureString { get; set; }

        byte[] BinaryValue { get; set; }

        void SetValue(string value);
    }
}