using System;

namespace Stardust.Nucleus
{
    public interface IConfigurator
    {
        IBindContext<T> Bind<T>();
        IBindContext BindAsGeneric(Type genericUnboundType);
        void RemoveAll();
        IUnbindContext<T> UnBind<T>();
    }
}