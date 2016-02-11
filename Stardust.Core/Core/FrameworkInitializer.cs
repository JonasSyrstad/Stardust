using System.Activities.Expressions;
using System.Diagnostics;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Stardust.Core.Security;
using Stardust.Nucleus;
using Stardust.Nucleus.Configuration;
using Stardust.Particles;

namespace Stardust.Core
{
    public class FrameworkInitializer
    {
        /// <summary>
        /// This is called by WebActivator in web applications and must be
        /// called manually when used in other application types. This method initializes
        /// the framework. If you only use the IoC container and not the entire
        /// SOA framework add stardust.OnlyIoC= true to web.config ->
        /// appSettings in order to load only the needed components. If you use
        /// the SOA framework inherit from 
        /// <see cref="CoreFrameworkBlueprint" /> to apply your own
        /// bindings as well as binding the framework it self. Load the binding
        /// configurations by calling Resolver.LoadModuleConfiguration()
        /// </summary>
        public static void InitializeModules()
        {
            try
            {
                var c = ConfigurationHelper.Configurations.Value;
                if (c != null && c.BindingConfigurationType != null)
                {
                    Resolver.LoadModuleConfiguration();
                    var appInitializer = Resolver.Activate<IStardustWebInitializer>();
                    if(appInitializer.IsInstance())
                        DynamicModuleUtility.RegisterModule(typeof(StardustWebInitializer));
                }
            }
            catch
            {
            }
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.OnlyIoC") == "true")
                Resolver.LoadModuleConfiguration<CoreFrameworkBlueprint>();
            else
                new WebServerConfiguration().PrepWebServer(null);
        }
    }

    public class StardustWebInitializer:IHttpModule
    {
        private static readonly object triowing = new object();

        private static bool initialized = false;
        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application </param>
        public void Init(HttpApplication context)
        {
            if(initialized) return;
            lock (triowing)
            {
                if(initialized) return;
                Resolver.Activate<IStardustWebInitializer>().Initialize(new ConfigWrapper(context));
            }
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
        }
    }

    public interface IStardustWebInitializer
    {
        void Initialize(ConfigWrapper instance);
    }

    public sealed class ConfigWrapper:IConfigWrapper
    {
        private readonly HttpApplication context;

        internal ConfigWrapper(HttpApplication context)
        {
            this.context = context;
        }

        HttpApplication IConfigWrapper.Context
        {
            get
            {
                return context;
            }
        }
    }

    public interface IConfigWrapper
    {
        HttpApplication Context { get; }
    }
}