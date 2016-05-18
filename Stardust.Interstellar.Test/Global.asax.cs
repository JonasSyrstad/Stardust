using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Stardust.Core.Service.Web;
using Stardust.Interstellar.Rest.ServiceWrapper;
using Stardust.Interstellar.Rest.Test;

namespace Stardust.Interstellar.Test
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ServiceFactory.CreateServiceImplementationForAllInCotainingAssembly<ITestApi>();
            ServiceFactory.FinalizeRegistration();

            this.LoadBindingConfiguration<TestBlueprint>();
           
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }

    public class TestBlueprint:Blueprint
    {
        /// <summary>Place your bindings here</summary>
        protected override void DoCustomBindings()
        {
            base.DoCustomBindings();
            Configurator.Bind<ITestApi>().To<TestApiImp>().SetTransientScope();
        }
    }

    public class TestApiImp : ITestApi
    {
        public string Apply1(string id, string name)
        {
            return string.Join("-", id, name);
        }

        public string Apply2(string id, string name, string item3)
        {
            return string.Join("-", id, name,item3);
        }

        public string Apply3(string id, string name, string item3, string item4)
        {
            return string.Join("-", id, name,item3,item4);
        }

        public void Put(string id, DateTime timestamp)
        {
            return;
        }

        public Task<StringWrapper> ApplyAsync(string id, string name, string item3, string item4)
        {
            return Task.FromResult(new StringWrapper {Value = string.Join("-", id, name, item3, item4) });
        }

        public Task PutAsync(string id, DateTime timestamp)
        {
            return Task.FromResult(2);
        }
    }
}
