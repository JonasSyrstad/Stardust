using System.Collections.Generic;
using System.Linq;
using Stardust.Interstellar.Endpoints;
using Stardust.Nucleus;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business
{
    public static class ConfigReaderFactory
    {
        public static List<KeyValuePair<string, string>> GetAvailableBindingTypes(IEnumerable<string> addedBindings )
        {
            BindingFactory.Initialize();
            var dat = from b in Resolver.GetImplementationsOf<IBindingCreator>()
                where b.Key != "sts" && !addedBindings.Contains(b.Key)
                select b;
            return dat.ToList();
        }

        public static IUserFacade GetUserFacade()
        {
            return Resolver.Activate<IUserFacade>();
        }

        public static IConfigUser CurrentUser
        {
            get
            {
                return (IConfigUser) ContainerFactory.Current.Resolve(typeof(IConfigUser),Scope.Context, () => GetUserFacade().GetUser(ContainerFactory.CurrentPrincipal.Identity.Name.GetUsername()));
            }
        }

        private static string GetUsername(this string userName)
        {
            var splitted = userName.Split('\\');
            if (splitted.Length == 1) return userName;
            return splitted[1];
        }
    }
}
