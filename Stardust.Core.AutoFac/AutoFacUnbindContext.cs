using System;
using Autofac;
using Stardust.Nucleus;

namespace Stardust.Core.AutoFac
{
    internal class AutoFacUnbindContext<T> : IUnbindContext<T>
    {
        private readonly ContainerBuilder Configuration;

        public AutoFacUnbindContext(ContainerBuilder configuration)
        {
            Configuration = configuration;
        }

        public void From<TImplementation>() where TImplementation : T
        {
            throw new NotImplementedException();
        }

        public void From<TImplementation>(string identifier) where TImplementation : T
        {
            throw new NotImplementedException();
        }

        public void From<TImplementation>(Enum identifier) where TImplementation : T
        {
            throw new NotImplementedException();
        }

        public void All()
        {
            
        }

        public IBindContext<T> AllAndBind()
        {
            return new AfBindContext<T>(Configuration);
        }

        public void From(Enum typeEnum)
        {
            
        }

        public void From(string named)
        {
            
        }

        public IBindContext<T> FromAndRebind(Enum typeEnum)
        {
            return new AfBindContext<T>(Configuration);
        }

        public IBindContext<T> FromAndRebind(string named)
        {
            return new AfBindContext<T>(Configuration);
        }
    }
}