using System.Collections.Generic;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business
{
    public interface IUserFacade
    {
        bool IsUserInRole(string username, string roleName);
        string[] GetUserRoles(string username);
        string[] GetUsersInRole(string roleName);

        IEnumerable<IConfigUser> GetUsers();
        void CreateUser(IConfigUser newUser);
        void UpdateUser(ConfigUser model);
        IConfigUser GetUser(string id);
    }
}