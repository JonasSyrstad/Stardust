using System;

namespace Stardust.Nucleus
{
    public interface IUnbindContext<T>
    {
        /// <summary>
        /// Removes the binding if existing. 
        /// Does not throw an exception if there is not binding since the intention is achieved
        /// </summary>
        void From<TImplementation>() where TImplementation : T;

        void From<TImplementation>(string identifier) where TImplementation : T;
        void From<TImplementation>(Enum identifier) where TImplementation : T;
        void All();
        IBindContext<T> AllAndBind();
        void From(Enum typeEnum);
        void From(string named);
        IBindContext<T> FromAndRebind(Enum typeEnum);
        IBindContext<T> FromAndRebind(string named);
    }
}