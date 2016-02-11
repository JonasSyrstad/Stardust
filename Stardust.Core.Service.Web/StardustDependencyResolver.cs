using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Stardust.Nucleus;

namespace Stardust.Core.Service.Web
{
    internal class StardustDependencyResolver : IDependencyResolver
    {
        public IDependencyScope BeginScope()
        {
            return(IDependencyScope)ContainerFactory.Current.Resolve(typeof(IDependencyResolver), Scope.Singleton,CreateDependencyScope);
        }

        private object CreateDependencyScope()
        {
            return new StardustDependencyScope();
        }

        public object GetService(Type serviceType)
        {
            return Resolver.Activate(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return Resolver.GetAllInstances(serviceType);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                GC.SuppressFinalize(this);
            
        }

        ~StardustDependencyResolver()
        {
            Dispose(false);
        }
    }
}