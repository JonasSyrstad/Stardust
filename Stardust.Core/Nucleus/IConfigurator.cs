using System;

namespace Stardust.Nucleus
{
    public interface IConfigurator
    {
        IBindContext<T> Bind<T>();
        IBindContext Bind(Type type);
        IBindContext BindAsGeneric(Type genericUnboundType);
        void RemoveAll();
        IUnbindContext<T> UnBind<T>();
    }
}