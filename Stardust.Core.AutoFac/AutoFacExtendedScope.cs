using System;
using Autofac;
using Stardust.Nucleus.ContextProviders;
using Stardust.Particles;

namespace Stardust.Core.AutoFac
{
    internal class AutoFacExtendedScope : IExtendedScopeProvider
    {
        private readonly IContainer Container;
        private readonly IExtendedScopeProvider Scope;
        private bool IsDisposing;
        private ILifetimeScope AfScope;

        public AutoFacExtendedScope(IContainer container, IExtendedScopeProvider scope)
        {
            Container = container;
            Scope = scope;
            AfScope = container.BeginLifetimeScope();
        }

        public void Bind(Type type, object toInstance)
        {
            Scope.Bind(type, toInstance);
        }

        public void InvalidateBinding(Type type)
        {
            Scope.InvalidateBinding(type);
            AfScope.Resolve(type);

        }

        public void KillEmAll()
        {
            Scope.KillEmAll();
            if (AfScope.IsInstance())
                AfScope.Dispose();
            AfScope = Container.BeginLifetimeScope();
        }

        public object Resolve(Type type)
        {
            var item = AfScope.ResolveOptional(type);
            if (item.IsNull())
                item = Scope.Resolve(type);
            return item;
        }

        public string ScopeId
        {
            get { return Scope.ScopeId; }
        }

        public IScopeProvider EndScope()
        {
            AfScope.Dispose();
            AfScope = null;
            return Scope.EndScope();
        }

        public IExtendedScopeProvider EndCurrentAndBeginNew()
        {
            var newScope = new AutoFacExtendedScope(Container, Scope.EndCurrentAndBeginNew());
            return newScope;
        }
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposing) return;
            IsDisposing = true;
            if (disposing) GC.SuppressFinalize(this);
            Scope.Dispose();
            if (AfScope.IsInstance())
            {
                AfScope.Dispose();
                AfScope = null;
            }
        }

        ~AutoFacExtendedScope()
        {
            Dispose(false);
        }
    }
}