using System;
using System.IO;
using Stardust.Particles;

namespace Stardust.Nucleus.ContextProviders
{
    public abstract class ScopeProviderBase : IScopeProvider
    {
        protected virtual IExtendedScopeProvider ChildScope
        {
            get { return GetExtendedScopeProvider(); }
            set
            {
                var item = GetExtendedScopeProvider();
                if (item.IsInstance()) throw new InvalidOperationException("Child provider already exists");
                DoBind(typeof(IExtendedScopeProvider), value);
            }
        }

        private IExtendedScopeProvider GetExtendedScopeProvider()
        {
            return (IExtendedScopeProvider)DoResolve(typeof(IExtendedScopeProvider));
        }

        protected abstract object DoResolve(Type type);

        public virtual IExtendedScopeProvider BeginExtendedScope()
        {
            IExtendedScopeProvider extScope;
            if (!TryBeginExtendedScope(out extScope)) throw new InvalidDataException("Unable to create scope, child scope exists");
            return extScope;
        }

        public virtual bool TryBeginExtendedScope(out IExtendedScopeProvider childScope)
        {
            bool result;
            if (ChildScope.IsInstance())
            {
                result = false;
                childScope = null;
            }
            else
            {
                ChildScope = (IExtendedScopeProvider)Activator.CreateInstance(Container.ExtendedContextProvider, this);
                result = true;
                childScope = ChildScope;
            }
            return result;
        }

        public virtual IScopeProvider EndScope(IExtendedScopeProvider scope)
        {
            ValidateEndScopeWhitException(scope);
            ChildScope.Dispose();
            InvalidateBinding(typeof(IExtendedScopeProvider));
            return this;
        }

        protected void ValidateEndScopeWhitException(IExtendedScopeProvider scope)
        {
            if (ChildScope.IsNull())
                throw new InvalidDataException("Unable to terminate scope, the current scope does not have a child scope");
            if (scope.IsNull())
                throw new NullReferenceException("Provided scope is null");
            if (ChildScope.ScopeId != scope.ScopeId)
                throw new InvalidDataException("Unable to terminate scope, actual scope not the same as provided scope");
        }

        public virtual bool TryEndScope(IExtendedScopeProvider scope, out IScopeProvider parentScope)
        {
            if (!ValidateScope(scope, out parentScope)) return false;
            ChildScope.Dispose();
            parentScope = this;
            InvalidateBinding(typeof(IExtendedScopeProvider));
            return true;
        }

        protected bool ValidateScope(IExtendedScopeProvider scope, out IScopeProvider parentScope)
        {
            parentScope = null;
            if (ChildScope.IsNull()) return false;
            if (scope.IsNull()) return false;
            return ChildScope.ScopeId == scope.ScopeId;
        }

        public void Bind(Type type, object toInstance)
        {
            if (ChildScope.IsInstance())
            {
                ChildScope.Bind(type, toInstance);
            }
            else
            {
                DoBind(type, toInstance);
            }
        }

        protected abstract void DoBind(Type type, object toInstance);

        public abstract void InvalidateBinding(Type type);

        public abstract void KillEmAll();

        public object Resolve(Type type)
        {
            if (ChildScope.IsInstance())
            {
                var item = ChildScope.Resolve(type);
                if (item.IsInstance())
                    return item;
            }
            return DoResolve(type);
        }
    }
}