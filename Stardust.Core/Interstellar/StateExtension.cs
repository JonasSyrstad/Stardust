using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Principal;

namespace Stardust.Interstellar
{
    /// <summary>
    /// Helpers on top of the <see cref="IRuntime" /> and
    /// <see cref="IStateStorageTask"/>
    /// </summary>
    public static class StateExtension
    {
        /// <summary>
        /// Persists the WIF <see cref="BootstrapContext" /> to the store
        /// </summary>
        /// <param name="Runtime">
        /// the current <see cref="IRuntime"/> instance
        /// </param>
        /// <param name="context">
        /// the <see cref="BootstrapContext"/> to persist
        /// </param>
        public static void SetBootstrapContext(this IRuntime runtime, BootstrapContext context)
        {
            runtime.GetStateStorageContainer().TryAddStorageItem(context);
        }

        /// <summary>
        /// Gets
        /// </summary>
        /// <param name="Runtime">the current <see cref="IRuntime"/> instance</param>
        /// <returns>the BootstrapContext for the current runtime instance</returns>
        public static BootstrapContext GetBootstrapContext(this IRuntime runtime)
        {
            BootstrapContext token;
            return runtime.GetStateStorageContainer().TryGetItem(out token) ? token : null;
        }

        /// <summary>
        /// Gets the generic principal for the current scope
        /// </summary>
        /// <param name="runtime">the current <see cref="IRuntime"/> instance</param>
        /// <returns>The principal</returns>
        public static IPrincipal GetCurrentPrincipal(this IRuntime runtime)
        {
            IPrincipal principal;
            return runtime.GetStateStorageContainer().TryGetItem(out principal) ? principal : null; 
        }

        /// <summary>
        /// Gets the claims principal for the current scope
        /// </summary>
        /// <param name="runtime">the current <see cref="IRuntime"/> instance</param>
        /// <returns>The principal</returns>
        public static ClaimsPrincipal GetCurrentClaimsPrincipal(this IRuntime runtime)
        {
            IPrincipal principal;
            if (runtime.GetStateStorageContainer().TryGetItem(out principal)) return principal as ClaimsPrincipal;
            return null;
        }

        /// <summary>
        /// Gets the claims identity for the current scope
        /// </summary>
        /// <param name="runtime">the current <see cref="IRuntime"/> instance</param>
        /// <returns>The identity of the user</returns>
        public static ClaimsIdentity GetCurrentClaimsIdentity(this IRuntime runtime)
        {
            IPrincipal principal;
            if (runtime.GetStateStorageContainer().TryGetItem(out principal)) return principal.Identity as ClaimsIdentity;
            return null;
        }

        /// <summary>
        /// Persists the <paramref name="principal"/> to the state container.
        /// </summary>
        /// <param name="runtime">the current <see cref="IRuntime"/> instance</param>
        /// <param name="principal">the principal to store</param>
        public static void SetCurrentPrincipal(this IRuntime runtime, IPrincipal principal)
        {
            runtime.GetStateStorageContainer().TryAddStorageItem(principal);
        }
    }
}