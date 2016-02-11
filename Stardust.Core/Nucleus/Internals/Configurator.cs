using System;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Nucleus.Internals
{
    class Configurator : IConfigurator
    {
        private readonly IConfigurationKernel Kernel;

        public Configurator(IConfigurationKernel kernel)
        {
            Kernel = kernel;
        }

        public IBindContext<T> Bind<T>()
        {
            return new BindContext<T>(Kernel);
        }

        public IBindContext BindAsGeneric(Type genericUnboundType)
        {
            return new BindContext(Kernel, genericUnboundType,true);
        }

        public void RemoveAll()
        {
            Kernel.UnbindAll();
        }

        public IUnbindContext<T> UnBind<T>()
        {
            return new UnbindContext<T>(Kernel);
        }
    }
}