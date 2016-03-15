using System;
using System.Collections.Generic;

namespace Stardust.Nucleus.TypeResolver
{
    /// <summary>
    /// Finds implementations from assemblies loaded into the <see cref="AppDomain"/>
    /// </summary>
    public interface IAssemblyScanningTypeResolver
    {
        /// <summary>
        /// Locates implementations of the Type within Assemblies loaded into the <see cref="AppDomain"/>
        /// </summary>
        /// <param name="type">the type to find implementations of</param>
        /// <returns>a list of implementations or subtypes</returns>
        IEnumerable<IScopeContext> LocateInLoadedAssemblies(Type type);

        IConfigurationKernel ConfigurationKernel { get; set; }
    }
}