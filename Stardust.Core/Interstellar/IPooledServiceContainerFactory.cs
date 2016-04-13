namespace Stardust.Interstellar
{
    /// <summary>
    /// An abstraction to wrap messaging technologies bind the common Stardust invocation and initialization framework
    /// </summary>
    public interface IPooledServiceContainerFactory
    {
        /// <summary>
        /// Creates a Pooled service container for a given service interface. 
        /// </summary>
        /// <param name="runtime"></param>
        /// <typeparam name="TService">The service contract</typeparam>
        /// <returns>A wrapper around the service</returns>
        IPooledServiceContainer<TService> CreatePooledServiceProxy<TService>(IRuntime runtime) where TService : class;

        /// <summary>
        /// Creates a Pooled service container for a given service interface. 
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="serviceName">The name used to look in the configuration system with</param>
        /// <typeparam name="TService">The service contract</typeparam>
        /// <returns>A wrapper around the service</returns>
        IPooledServiceContainer<TService> CreatePooledServiceProxy<TService>(IRuntime runtime, string serviceName) where TService : class;
    }
}
