using System;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Nucleus.Internals
{
    internal class Configurator : IConfigurator
    {
        private readonly IConfigurationKernel kernel;

        internal Configurator(IConfigurationKernel kernel)
        {
            this.kernel = kernel;
        }

        IBindContext<T> IConfigurator.Bind<T>()
        {
            return new BindContext<T>(kernel);
        }

        IBindContext IConfigurator.Bind(Type type)
        {
            return new BindContext(kernel,type);
        }

        IBindContext IConfigurator.BindAsGeneric(Type genericUnboundType)
        {
            return new BindContext(kernel, genericUnboundType,true);
        }

        void IConfigurator.RemoveAll()
        {
            kernel.UnbindAll();
        }

        IUnbindContext<T> IConfigurator.UnBind<T>()
        {
            return new UnbindContext<T>(kernel);
        }
    }
}