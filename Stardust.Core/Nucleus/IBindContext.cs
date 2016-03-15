using System;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Nucleus
{
    public interface IBindContext
    {
        /// <summary>
        /// Binds an implementation to its base class or interface
        /// </summary>
        IScopeContext To(Type type, string identifier = "default");

        IScopeContext To(Type type, Enum identifier);
    }
    public interface IBindContext<T> : IBindContext
    {
        /// <summary>
        /// Binds an implementation to its base class or interface
        /// </summary>
        IScopeContext To<TImplementation>() where TImplementation : T;

        /// <summary>
        /// Binds an implementation to its base class or interface
        /// </summary>
        IScopeContext To<TImplementation>(string identifier) where TImplementation : T;

        /// <summary>
        /// Binds an instace of an implementation to its base class or interface
        /// </summary>
        IScopeContext ToInstance(T instance, string identifier = TypeLocatorNames.DefaultName);

        /// <summary>
        /// Binds an implementation to its base class or interface
        /// </summary>
        IScopeContext To<TImplementation>(Enum identifier) where TImplementation : T;

        /// <summary>
        /// Binds the service to it self
        /// </summary>
        IScopeContext ToSelf();

        /// <summary>
        /// Binds the service to it self
        /// </summary>
        IScopeContext ToSelf(string identifier);

        /// <summary>
        /// Binds the service to it self
        /// </summary>
        IScopeContext ToSelf(Enum identifier);

        IScopeContext ToConstructor(Func<object> creator);
        IScopeContext ToConstructor(Func<object> creator, string identifier);

        
    }
}