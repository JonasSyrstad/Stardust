using System.IdentityModel.Services.Tokens;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Wormhole;

namespace Stardust.Core.Service.Web
{
    /// <summary>
    /// Set up additional features of Stardust
    /// </summary>
    public interface ISetupContext
    {
        /// <summary>
        /// Binds the processing interceptor to gracefully tear down the
        /// <see cref="IRuntime" /> instance after each http request is
        /// completed and handle error logging. Also sets the
        /// <paramref name="application" /> to use passive federation for user
        /// authentication.
        /// </summary>
        /// <typeparam name="T">
        /// The claims authentication manager to use for inspecting incoming
        /// security tokens
        /// </typeparam>
        /// <param name="tokenHandler">
        /// the Session security token handler to use. if <see langword="null"/>
        /// it uses <see cref="MachineKeySessionSecurityTokenHandler"/> as
        /// default
        /// </param>
        IClaimsSetupContext MakeClaimsAware<T>(SessionSecurityTokenHandler tokenHandler = null) where T : ClaimsAuthenticationManager;

        /// <summary>
        /// Binds the processing interceptor to gracefully tear down the
        /// <see cref="IRuntime"/> instance after each http request is completed and handle
        /// error logging. Also sets the <paramref name="application" /> to use
        /// passive federation for user authentication.
        /// </summary>
        /// <param name="tokenHandler">
        /// the Session security token handler to use. if <see langword="null"/>
        /// it uses <see cref="MachineKeySessionSecurityTokenHandler"/> as
        /// default
        /// </param>
        IClaimsSetupContext MakeClaimsAware(SessionSecurityTokenHandler tokenHandler = null);

        

        /// <summary>
        /// Binds the processing interceptor to gracefully tear down the
        /// <see cref="IRuntime"/> instance after each http request is completed and handle
        /// error logging. Also sets the <paramref name="application" /> to use
        /// passive federation for user authentication.
        /// </summary>
        /// <param name="tokenHandler">
        /// the Session security token handler to use. if <see langword="null"/>
        /// it uses <see cref="MachineKeySessionSecurityTokenHandler"/> as
        /// default
        /// </param>
        /// <typeparam name="TAuth">The claims authentication manager to use for inspecting incoming security tokens</typeparam>
        /// <typeparam name="TCache">the securitytoken cache to use, for reducing fedAuth cookie sizes</typeparam>
        IClaimsSetupContext MakeClaimsAware<TAuth, TCache>(SessionSecurityTokenHandler tokenHandler = null)
            where TAuth : ClaimsAuthenticationManager
            where TCache : SessionSecurityTokenCache;

        /// <summary>
        /// Loads the provided map settings
        /// </summary>
        /// <typeparam name="T">The mapping definitions to use</typeparam>
        /// <returns></returns>
        ISetupContext LoadMapDefinitions<T>() where T : IMappingDefinition, new();

        /// <summary>
        /// Loads a binding configuration into the IOC resolver
        /// </summary>
        /// <typeparam name="T">the binding to load</typeparam>
        /// <returns></returns>
        ISetupContext LoadBindings<T>() where T : IBlueprint, new();
    }
}