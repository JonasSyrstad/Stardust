using System;
using System.Collections.Concurrent;
using Stardust.Particles;

namespace Stardust.Nucleus.ContextProviders
{
    internal sealed class ExtendedScopeProvider : IExtendedScopeProvider
    {
        private readonly ConcurrentDictionary<Type, object> ScopeStore = new ConcurrentDictionary<Type, object>();
        private Guid Id;
        private IScopeProvider Parent;
        private bool IsDisposing;

        public ExtendedScopeProvider(IScopeProvider parent)
        {
            Id = Guid.NewGuid();
            Parent = parent;
        }

        /// <summary>
        /// adds an instance to the scope
        /// </summary>
        /// <param name="type">the type of object to add to the scope</param>
        /// <param name="toInstance">An instance or an instance of a derived class of the given <paramref name="type"/></param>
        public void Bind(Type type, object toInstance)
        {
            ScopeStore.TryAdd(type, toInstance);
        }

        /// <summary>
        /// removes the binding to the type if any.
        /// </summary>
        /// <param name="type">The type to remove from the scope</param>
        public void InvalidateBinding(Type type)
        {

            object removedItem;
            ScopeStore.TryRemove(type, out removedItem);

        }

        /// <summary>
        /// Invalidates all bindings within the scope
        /// </summary>
        public void KillEmAll()
        {
            ScopeStore.Clear();
        }

        /// <summary>
        /// Resolves the type from the scope. 
        /// </summary>
        /// <param name="type"></param>
        /// <returns>an instance of the given <paramref name="type"/> or null if unbound</returns>
        public object Resolve(Type type)
        {
            object value;
            ScopeStore.TryGetValue(type, out value);
            return value;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if(IsDisposing) return;
            IsDisposing = true;
            if (disposing) GC.SuppressFinalize(this);

            IScopeProvider item;
            if (Parent.IsInstance())
                Parent.TryEndScope(this, out item);
            if(ScopeStore.IsNull()) return;
            foreach (var storeItem in ScopeStore)
            {
                storeItem.Value.TryDispose();
            }
            ScopeStore.Clear();
        }

        ~ExtendedScopeProvider()
        {
            Dispose(false);
        }

        public string ScopeId
        {
            get { return Id.ToString(); }
        }

        public IScopeProvider EndScope()
        {
            if (Parent.IsInstance())
                Parent.EndScope(this);
            return Parent;
        }

        public IExtendedScopeProvider EndCurrentAndBeginNew()
        {
            var parent = Parent;
            Parent.EndScope(this);
            return parent.BeginExtendedScope();
        }
    }
}