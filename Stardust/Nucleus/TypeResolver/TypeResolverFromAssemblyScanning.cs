using System;
using System.Collections.Generic;

namespace Stardust.Nucleus.TypeResolver
{
    internal class TypeResolverFromAssemblyScanning : IAssemblyScanningTypeResolver
    {
        public IEnumerable<IScopeContext> LocateInLoadedAssemblies(Type type)
        {
            return new List<IScopeContext>();
        }
    }
}
