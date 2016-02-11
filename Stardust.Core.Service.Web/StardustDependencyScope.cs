using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Stardust.Nucleus;
using Stardust.Nucleus.ContextProviders;

namespace Stardust.Core.Service.Web
{
    internal sealed class StardustDependencyScope : IDependencyScope
    {
        private IExtendedScopeProvider ScopeContainer;

        internal StardustDependencyScope()
        {
            ScopeContainer = ContainerFactory.Current.GetProvider(Scope.Context).BeginExtendedScope();
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
                GC.SuppressFinalize(this);
            ScopeContainer.EndScope();
            ScopeContainer = null;
        }

        /// <summary>
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        ~StardustDependencyScope()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public object GetService(Type serviceType)
        {
            return Resolver.Activate(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return Resolver.GetAllInstances(serviceType);
        }
    }
}