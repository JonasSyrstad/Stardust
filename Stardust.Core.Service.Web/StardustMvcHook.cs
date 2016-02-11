using System.Web;
using Owin;
using Stardust.Nucleus;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.WsFederation;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;

namespace Stardust.Core.Service.Web
{
    /// <summary>
    /// Configures the Stardust web framework
    /// </summary>
    public static class StardustMvcHook
    {
        /// <summary>
        /// Loads the provided binding configuration and binds the Stardust web framework
        /// </summary>
        /// <typeparam name="T">the type of binding configuration to use</typeparam>
        /// <param name="application">the current application instance</param>
        /// <returns>A context for you to do additional initializations</returns>
        public static ISetupContext LoadBindingConfiguration<T>(this HttpApplication application) where T : IBlueprint, new()
        {
            Resolver.LoadModuleConfiguration<T>();
        return application.CreateContext().BindItAll();
        }

        private static IStardustInternalSetupContext CreateContext(this HttpApplication application)
        {
            return new BaseBinderContext(application);
        }

        public static ISetupContext AddMvcHooks(this ConfigWrapper application)
        {
            return new BaseBinderContext(((IConfigWrapper)application).Context).BindItAll();
        }

        public static void MakeOAuthAware(this IAppBuilder builder)
        {
            var metadataFormat = ConfigurationManagerHelper.GetValueOnKey("stardust.FederationMetadataFormat");
            if (metadataFormat.IsNullOrWhiteSpace()) metadataFormat = "http://{0}/federationmetadata/2007-06/federationmetadata.xml";
            builder.UseWsFederationAuthentication(new WsFederationAuthenticationOptions
                                                      {
                                                          Wtrealm = IdentitySettings.Realm,
                                                          MetadataAddress = string.Format(metadataFormat, RuntimeFactory.Current.Context.GetEnvironmentConfiguration().GetConfigParameter("IssuerMetadataAddress")),
                                                          AuthenticationMode = AuthenticationMode.Passive,
                                                      });

        }

        private static IdentitySettings IdentitySettings
        {
            get
            {
                return RuntimeFactory.Current.Context.GetServiceConfiguration().IdentitySettings;
            }
        }

        /// <summary>
        /// Loads the provided binding configuration and binds the Stardust web framework
        /// </summary>
        /// <param name="application">the current application instance</param>
        /// <param name="bindingConfiguration">the binding configuration to use</param>
        /// <returns>A context for you to do additional initializations</returns>
        public static ISetupContext LoadBindingConfiguration(this HttpApplication application, IBlueprint bindingConfiguration) 
        {
            Resolver.LoadModuleConfiguration(bindingConfiguration);
            return application.CreateContext().BindItAll();
        }

        /// <summary>
        /// binds the Stardust web framework
        /// </summary>
        /// <param name="application">the current application instance</param>
        /// <returns>A context for you to do additional initializations</returns>
        public static ISetupContext LoadBindingConfiguration(this HttpApplication application)
        {
            return application.CreateContext().BindItAll();
        }
    }
}