using System.Collections.Generic;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Web.Models
{
    public class AdministratorsModel
    {
        public List<IConfigUser> CurrentAdministrators { get; set; }
        public List<IConfigUser> AvaliableUsers { get; set; }

        public string[] PostedUserIds { get; set; }
    }
}