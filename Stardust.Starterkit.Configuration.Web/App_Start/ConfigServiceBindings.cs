using Stardust.Interstellar;
using Stardust.Interstellar.Endpoints;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Business.CahceManagement;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Web
{
    public class ConfigServiceBindings : Blueprint
    {
        protected override void DoCustomBindings()
        {
            Resolver.Bind<IBindingCreator>().To<BasicBindingCreator>("oracle").SetThreadScope();
            Resolver.Bind<IConfigSetTask>().To<ConfigSetTask>().SetRequestResponseScope().AllowOverride = false;
            Resolver.Bind<IRepositoryFactory>().To<RepositoryFactory>().SetSingletonScope().AllowOverride = false;
            Resolver.Bind<IUserFacade>().To<UserFacade>().SetRequestResponseScope().AllowOverride = false;
            Resolver.Bind<ICacheManagementService>().To<CacheManagementService>().SetTransientScope();
            Resolver.Bind<ICacheManagementWrapper>().To<NullCacheManagementWrapper>().SetTransientScope();
            Resolver.Bind<ICacheManagementWrapper>().To<AzureRedisFabricCacheManager>("Azure").SetTransientScope();
        }
    }
}