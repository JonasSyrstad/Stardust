using System.Linq;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business
{
    public static class AuthorizationExtentions
    {
        public static bool UserHasAccessTo(this IConfigSet self)
        {
            if (ConfigReaderFactory.CurrentUser.AdministratorType == AdministratorTypes.SystemAdmin) return true;
            var cnt= (from u in self.Administrators where u.NameId == ConfigReaderFactory.CurrentUser.NameId select u).Count();
            return cnt > 0;
        }

        public static bool UserHasAccessTo(this IEnvironment self)
        {
            return self.ConfigSet.UserHasAccessTo();
        }

        public static bool UserHasAccessTo(this IServiceDescription self)
        {
            return self.ConfigSet.UserHasAccessTo();
        }

        public static bool UserHasAccessTo(this IServiceHostSettings self)
        {
            return self.ConfigSet.UserHasAccessTo();
        }

        public static bool UserHasAccessTo(this IEnvironmentParameter self)
        {
            return self.Environment.UserHasAccessTo();
        }

        public static bool UserHasAccessTo(this ISubstitutionParameter self)
        {
            return self.Environment.UserHasAccessTo();
        }

        public static bool UserHasAccessTo(this IServiceHostParameter self)
        {
            return self.ServiceHost.UserHasAccessTo();
        }

        public static bool UserHasAccessTo(this IEndpoint self)
        {
            return self.ServiceDescription.UserHasAccessTo();
        }

        public static bool UserHasAccessTo(this IEndpointParameter self)
        {
            return self.Endpoint.UserHasAccessTo();
        }
    }
}