using System;
using System.Collections.Generic;

namespace Stardust.Nucleus.TypeResolver
{
    /// <summary>
    /// Finds implementation bindings from the configuration section. 
    /// </summary>
    public interface IConfigurationTypeResolver
    {
        /// <summary>
        /// Finds all implementation bindings for a <see cref="Type"/>
        /// </summary>
        /// <param name="type">The Type to find implementations for</param>
        /// <returns>a list of configured bindings.</returns>
        IEnumerable<IScopeContext> GetTypeBindingsFromConfig(Type type);
    }

}