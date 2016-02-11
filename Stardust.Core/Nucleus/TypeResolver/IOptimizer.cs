using System;
using System.Collections.Generic;

namespace Stardust.Nucleus.TypeResolver
{
    /// <summary>
    /// This provides an extention point to allow Stardust to utilize other type resolver frameworks under nea
    /// </summary>
    public interface IOptimizer
    {
        IScopeContext FindSubtypeOf(Type type, string implementationReference);
        void SetImplementationType(Type baseType, IScopeContext subType, string implementationReference);
        void RemoveImplementationType(Type baseType, IScopeContext subType, string implementationReference);
        void RemoveImplementationType(Type type);
        IEnumerable<KeyValuePair<string, string>> GetImplementationsOf(Type type);
        void RemoveImplementationType(Type baseType, string named);
        IEnumerable<IScopeContext> ResolveAllOf<T>();
        
        void AddBaseTypeIfNonExisting(Type baseType);
        
        void RemoveImplementation(Type baseType, string implementationReference);
        void RemoveAll();
        IEnumerable<IScopeContext> GetAllSubClassesOf(Type baseType);
    }
}