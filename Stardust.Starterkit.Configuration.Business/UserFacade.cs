using System;
using System.Collections.Generic;
using System.Linq;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business
{
    public class UserFacade : IUserFacade
    {
        private ConfigurationContext Repository;

        public UserFacade(IRepositoryFactory repository)
        {
            Repository = repository.GetRepository();
        }
        public bool IsUserInRole(string username, string roleName)
        {
            return (from u in Repository.ConfigUsers
                    where u.NameId.Equals(username, StringComparison.CurrentCultureIgnoreCase) && u.AdministratorType == roleName.ParseAsEnum<AdministratorTypes>()
                    select u).Count() == 1;
        }

        public string[] GetUserRoles(string username)
        {
            var data = (from u in Repository.ConfigUsers where u.NameId.Equals(username, StringComparison.CurrentCultureIgnoreCase) select u).ToArray();
            return (from r in data select r.AdministratorType.ToString()).ToArray();
        }

        public string[] GetUsersInRole(string roleName)
        {
            return
                (from u in Repository.ConfigUsers
                 where u.AdministratorType == roleName.ParseAsEnum<AdministratorTypes>()
                 select u.NameId).ToArray();
        }

        public IEnumerable<IConfigUser> GetUsers()
        {
            return Repository.ConfigUsers.ToList();
        }

        public void CreateUser(IConfigUser newUser)
        {
            var user = Repository.ConfigUsers.Create();
            user.FirstName = newUser.FirstName;
            user.LastName = newUser.LastName;
            user.NameId = newUser.NameId;
            user.AdministratorType = user.AdministratorType;
            Repository.SaveChanges();
        }

        public void UpdateUser(ConfigUser model)
        {
            var user = GetUser(model.NameId);
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            if(user.NameId!=ConfigReaderFactory.CurrentUser.NameId)
                user.AdministratorType = model.AdministratorType;
            Repository.SaveChanges();
        }

        public IConfigUser GetUser(string id)
        {
            if (id.IsNullOrWhiteSpace()) return null; 
            if (Repository.ConfigUsers.Count() == 0)
            {
                var user = Repository.ConfigUsers.Create();
                user.FirstName = "";
                user.LastName = "";
                user.NameId = id;
                user.AdministratorType=AdministratorTypes.SystemAdmin;
                Repository.SaveChanges();
            }
            return (from u in Repository.ConfigUsers
                    where u.NameId.Equals(id, StringComparison.CurrentCultureIgnoreCase)
                    select u).Single();
        }
    }
}