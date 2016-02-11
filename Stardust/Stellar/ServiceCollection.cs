using System.Collections;
using System.Collections.Generic;

namespace Stardust.Stellar
{
    public sealed class ServiceCollection : IServiceCollection
    {
        private readonly Dictionary<string, IConfigurationExtensionsCore> configurationExtensionsCores;

        public IConfigurationExtensionsCore this[string name]
        {
            get
            {
                IConfigurationExtensionsCore value;
                configurationExtensionsCores.TryGetValue(name, out value);
                return value;
            }
        }

        internal ServiceCollection(Dictionary<string, IConfigurationExtensionsCore> configurationExtensionsCores)
        {
            this.configurationExtensionsCores = configurationExtensionsCores;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IConfigurationExtensionsCore> GetEnumerator()
        {
            return configurationExtensionsCores.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal void Add(string serviceName, ConfigurationExtension configurationExtension)
        {
            configurationExtensionsCores.Add(serviceName, configurationExtension);
        }
    }

    public interface IServiceCollection : IEnumerable<IConfigurationExtensionsCore>
    {
        IConfigurationExtensionsCore this[string name] { get; }
    }
}