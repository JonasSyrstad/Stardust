using System.Collections.Generic;
using BrightstarDB.EntityFramework;

namespace Stardust.Starterkit.Configuration.Repository
{
    [Entity("ConfigUser")]
    public interface IConfigUser
    {
        [Identifier("http://stardust.com/configuration/Users", KeyProperties = new[] { "NameId" })]
        string Id { get; }

        string NameId { get; set; }

        [InverseProperty("Administrators")]
        ICollection<IConfigSet> ConfigSet { get; set; }

        string FirstName { get; set; }

        string LastName { get; set; }

        AdministratorTypes AdministratorType { get; set; }
    }

    public partial class ConfigUser
    {
        public override string ToString()
        {
            return string.Format("{0}, {1}", LastName, FirstName);
        }
    }
}