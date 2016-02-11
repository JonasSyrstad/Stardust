using Stardust.Nucleus;

namespace Stardust.Interstellar
{
    /// <summary>
    /// An abstraction to wrap messaging technologies bind the common Stardust invocation and initialization framework
    /// </summary>
    public interface IServiceContainerFactory
    {
        /// <summary>
        /// Creates a configured instance of a service container. 
        /// </summary>
        /// <typeparam name="TService">The service to wrap in a service container</typeparam>
        /// <param name="runtime">The current <see cref="IRuntime"/> instance</param>
        /// <param name="serviceName">The service name for lookup in the configuration system</param>
        /// <param name="scope">the OLM scope for the created instance.</param>
        /// <returns></returns>
        IServiceContainer<TService> CreateContainer<TService>(IRuntime runtime, string serviceName, Scope scope = Scope.Context);
    }
}
