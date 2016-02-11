using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BrightstarDB.EntityFramework;

namespace Stardust.Starterkit.Configuration.Repository
{
    [Entity("SubstitutionParameter")]
    public interface ISubstitutionParameter
    {
        [Identifier("http://stardust.com/configuration/Environments/Substitutions/", KeyProperties = new[] { "EnvironmentNameId", "Name" }, KeySeparator = "-")]
        string Id { get; }
        [RegularExpression(Constants.KeyValidator, ErrorMessage = "special characters are not  allowed.")]
        string Name { get; set; }

        string EnvironmentNameId { get; set; }

        IEnvironment Environment { get; set; }

        ISubstitutionParameter Parent { get; set; }

        [InverseProperty("Parent")]
        ICollection<ISubstitutionParameter> ChildParameters { get; set; }

        string ItemValue { get; set; }

        [Ignore]
        string Value { get; }

        [Ignore]
        bool IsRoot { get; }
        [Ignore]
        bool IsInherited { get; }

        bool IsSecure { get; set; }
    }
}