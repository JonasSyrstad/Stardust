using System;
using System.Web.Security;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Web.Providers
{

    static class UserExt
    {
        internal static string GetUsername(this string userName)
        {
            var splitted = userName.Split('\\');
            if (splitted.Length == 1) return userName;
            return splitted[1];
        }
    }
    public class ConfigRoleProvider:RoleProvider
    {

        protected IUserFacade UserFacade
        {
            get { return ConfigReaderFactory.GetUserFacade(); }
        }
        public override bool IsUserInRole(string username, string roleName)
        {
            return UserFacade.IsUserInRole(username.GetUsername(), roleName);
        }

        public override string[] GetRolesForUser(string username)
        {
            return UserFacade.GetUserRoles(username.GetUsername());
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            return UserFacade.GetUsersInRole(roleName);
        }

        public override string[] GetAllRoles()
        {
            return Enum.GetNames(typeof (AdministratorTypes));
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName { get; set; }
    }
}