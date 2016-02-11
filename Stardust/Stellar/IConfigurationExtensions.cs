using System.Collections.Generic;

namespace Stardust.Stellar
{
    /// <summary>
    /// Access the centralized configuration settings through this.
    /// Use Config[propertyName] for environment properties
    /// Use Config.Secure(propertyName) for encrypted environment properties
    /// Use Config.Service[serviceHostName] for service host settings.
    /// Cast the configuration item to dynamic.
    /// it uses the following convention: 
    /// {PropertyName} == Config[propertyName]
    /// Secure{PropertyName} == Config.Secure(propertyName)
    /// Service{ServiceHostName} == Config.Service[serviceHostName]
    /// </summary>
    public interface IConfigurationExtensions : IConfigurationExtensionsCore
    {

        IServiceCollection Service { get; }

    }
}