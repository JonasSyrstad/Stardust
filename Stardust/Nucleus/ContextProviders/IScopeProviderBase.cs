using System;

namespace Stardust.Nucleus.ContextProviders
{
    /// <summary>
    /// Provides an abstraction of the scope resolution providers
    /// </summary>
    public interface IScopeProviderBase
    {
        /// <summary>
        /// adds an instance to the scope
        /// </summary>
        /// <param name="type">the type of object to add to the scope</param>
        /// <param name="toInstance">An instance or an instance of a derived class of the given <paramref name="type"/></param>
        void Bind(Type type, object toInstance);

        /// <summary>
        /// removes the binding to the type if any.
        /// </summary>
        /// <param name="type">The type to remove from the scope</param>
        void InvalidateBinding(Type type);

        /// <summary>
        /// Invalidates all bindings within the scope
        /// </summary>
        void KillEmAll();

        /// <summary>
        /// Resolves the type from the scope. 
        /// </summary>
        /// <param name="type"></param>
        /// <returns>an instance of the given <paramref name="type"/> or null if unbound</returns>
        object Resolve(Type type);
    }
}