using Stardust.Nucleus;
using Stardust.Wormhole;

namespace Stardust.Core.Service.Web
{
    /// <summary>
    /// Provides extra setup options for claims aware applications. Note that calling LoadMapDefinitions 
    /// </summary>
    public interface IClaimsSetupContext 
    {
        /// <summary>
        /// Configures MVC to use the provided <paramref name="claimType"/> as antiforgery token. Call <see cref="MakeClaimsAware"/> before calling this.
        /// </summary>
        /// <param name="claimType"></param>
        /// <returns></returns>
        IClaimsSetupContext SetAntiForgeryToken(string claimType);

        /// <summary>
        /// Shortens the common cookie names. This reduces HTTP message sizes. Call <see cref="MakeClaimsAware"/> before calling this.
        /// </summary>
        /// <returns></returns>
        IClaimsSetupContext MinifyCommonCookieNames();

        IClaimsSetupContext MakeOAuthAwareService();

        IClaimsSetupContext MakeOAuthAwareWebsite();



        /// <summary>
        /// Loads the provided map settings
        /// </summary>
        /// <typeparam name="T">The mapping definitions to use</typeparam>
        /// <returns></returns>
        IClaimsSetupContext LoadMapDefinitions<T>() where T : IMappingDefinition, new();
        /// <summary>
        /// Loads a binding configuration into the IOC resolver
        /// </summary>
        /// <typeparam name="T">the binding to load</typeparam>
        /// <returns></returns>
        IClaimsSetupContext LoadBindings<T>() where T : IBlueprint, new();

        IClaimsSetupContext SecureWcfRestServices();
    }
}