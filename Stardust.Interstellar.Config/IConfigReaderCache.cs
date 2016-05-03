using System.Security.Cryptography.X509Certificates;

namespace Stardust.Interstellar.Config
{
    /// <summary>
    /// Provides caching for the config reader client.
    /// </summary>
    public interface IConfigReaderCache
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        void SetReaderProxy(IConfigReader reader);
        /// <summary>
        /// Retreives the configuration from the cache
        /// </summary>
        /// <param name="setName">name of the configuration set</param>
        /// <param name="environmentName">the environment</param>
        /// <param name="configSet">The cached configuration set</param>
        /// <returns>true if the cache contains the configuration</returns>
        bool TryGetConfig(string setName, string environmentName, out ConfigurationSet configSet);

        /// <summary>
        /// puts the configset in the cache
        /// </summary>
        /// <param name="setName">name of the configuration set</param>
        /// <param name="environmentName">the environment</param>
        /// <param name="configSet">The configuration set to cache</param>
        void TryAddConfig(string setName, string environmentName, ConfigurationSet configSet);
    }
}