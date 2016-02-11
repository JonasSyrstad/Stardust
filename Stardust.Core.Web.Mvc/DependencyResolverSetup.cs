using System.Web.Mvc;
using Stardust.Core.FactoryHelpers;

namespace Stardust.Core.Web.Mvc
{
    public static class DependencyResolverSetup
    {
        public static void Initialize()
        {
            Resolver.Bind<IControllerFactory>().To<DefaultControllerFactory>();
            Resolver.Bind<IDependencyResolver>().To<MvcControllerResolver>();
            Resolver.Bind<IControllerActivator>().To<ControllerActivator>();
            DependencyResolver.SetResolver(new MvcControllerResolver(DependencyResolver.Current));
        }
    }
}