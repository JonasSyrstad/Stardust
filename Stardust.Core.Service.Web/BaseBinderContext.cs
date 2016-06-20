using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using Stardust.Nucleus;


namespace Stardust.Core.Service.Web
{
    internal class BaseBinderContext : IStardustInternalSetupContext
    {
        private readonly HttpApplication Application;
        public BaseBinderContext(HttpApplication application)
        {
            Application = application;
        }

        public ISetupContext BindItAll()
        {
            Resolver.GetConfigurator().Bind<IControllerFactory>().To<StardustControllerFactory>().SetSingletonScope();
            Resolver.GetConfigurator().Bind<IControllerActivator>().To<StardustControllerActivator>().SetSingletonScope().DisableOverride();
            Resolver.GetConfigurator().Bind<IHttpActionInvoker>().To<ApiTeardownActionFilter>().SetSingletonScope();
            Resolver.GetConfigurator().Bind<IAsyncActionInvoker>().To<ControllerTearDownActionFilter>().SetSingletonScope();
            Resolver.GetConfigurator().Bind<IActionInvoker>().To<ControllerTearDownActionFilter>().SetSingletonScope();
            Resolver.GetConfigurator().Bind<IHttpControllerActivator>().To<StardustApiControllerActivator>().SetSingletonScope();
            //Resolver.GetConfigurator().Bind<System.Web.Http.Metadata.ModelMetadataProvider>().ToAssembly(typeof(System.Web.Http.Metadata.ModelMetadataProvider).Assembly);
            GlobalConfiguration.Configuration.DependencyResolver = new StardustDependencyResolver();
            ControllerBuilder.Current.SetControllerFactory(Resolver.Activate<IControllerFactory>());
            return new SetupContext(Application);
        }
    }
}