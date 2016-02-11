using System;
using System.Collections.Generic;

namespace Stardust.Nucleus.TypeResolver
{
    public interface IConfigurationKernel
    {
        IScopeContext Resolve(Type type, string named, bool skipAlternateResolving = false);

        IEnumerable<IScopeContext> ResolveAll(Type type);
        void Bind(Type concreteType, IScopeContext existingBinding, string identifier);
        void Unbind(Type type, string identifier);
        void UnbindAll(Type type);

        void Unbind(Type type, IScopeContext scopeContext, string identifier);
        IEnumerable<KeyValuePair<string, string>> ResolveList(Type type);

        /// <summary>
        /// </summary>
        void UnbindAll();
    }
}