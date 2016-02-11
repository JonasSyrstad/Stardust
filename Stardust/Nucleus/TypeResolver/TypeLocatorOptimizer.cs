using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Stardust.Nucleus.Internals;
using Stardust.Particles;

namespace Stardust.Nucleus.TypeResolver
{
    internal sealed class TypeLocatorOptimizer : IOptimizer
    {
        private class ImplementationContainer
        {
            internal readonly ConcurrentDictionary<string, ScopeContext> Typesets = new ConcurrentDictionary<string, ScopeContext>();
            internal ScopeContext Default { get; set; }
        }

        private readonly ConcurrentDictionary<string, ImplementationContainer> Typesets = new ConcurrentDictionary<string, ImplementationContainer>();

        private readonly ConcurrentDictionary<Type, ScopeContext> Defaults = new ConcurrentDictionary<Type, ScopeContext>();

        public IScopeContext FindSubtypeOf(Type type, string implementationReference)
        {
            if (implementationReference == TypeLocatorNames.DefaultName)
            {
                ScopeContext item;
                return Defaults.TryGetValue(type, out item) ? item : null;
            }
            if (type.IsNull()) throw new StardustCoreException("Type cannot be null");
            if (implementationReference.IsNullOrWhiteSpace()) throw new StardustCoreException("implementationReference cannot be null or whitespace");
            ImplementationContainer typeSet;
            return Typesets.TryGetValue(type.FullName, out typeSet) ? GetImplementationsOfBaseType(implementationReference, typeSet) : null;
        }

        public void SetImplementationType(Type baseType, IScopeContext subType, string implementationReference)
        {
            AddBaseTypeIfNonExisting(baseType);
            AddImplementationTypeIfNonExisting(baseType, subType, implementationReference);
        }

        public void RemoveImplementationType(Type baseType, IScopeContext subType, string implementationReference)
        {
            RemoveImplementation(baseType, implementationReference);
        }

        public void RemoveImplementationType(Type type)
        {
            ImplementationContainer removedItem;
            if (!Typesets.TryGetValue(type.FullName, out  removedItem)) return;
            removedItem.Typesets.Clear();
            removedItem.Default = null;
            ScopeContext removedDefault;
            Defaults.TryRemove(type, out removedDefault);
        }

        public IEnumerable<KeyValuePair<string, string>> GetImplementationsOf(Type type)
        {
            ImplementationContainer resolvedBase;
            if (!Typesets.TryGetValue(type.FullName, out  resolvedBase)) return new List<KeyValuePair<string, string>>();
            var dat = (
                       from i in resolvedBase.Typesets
                       select new KeyValuePair<string, string>(i.Key, i.Value.BoundType.FullName)).ToList();
            if (resolvedBase.Default != null)
                dat.Add(new KeyValuePair<string, string>(TypeLocatorNames.DefaultName, resolvedBase.Default.BoundType.FullName));
            return dat;
        }

        public void RemoveImplementationType(Type baseType, string named)
        {
            var item = Typesets[baseType.FullName];
            if (named == TypeLocatorNames.DefaultName)
            {
                item.Default = null;
                ScopeContext removedDefault;
                Defaults.TryRemove(baseType, out removedDefault);
                return;
            }
            var typeToRemove = (from i in item.Typesets
                                where i.Key == named
                                select i.Value).FirstOrDefault();
            if (typeToRemove.IsInstance())
                RemoveImplementationType(baseType, typeToRemove, named);
        }

        public IEnumerable<IScopeContext> ResolveAllOf<T>()
        {
            var type = Typesets[typeof(T).FullName];
            var vals = from t in type.Typesets select t.Value;
            var result = vals.ToList();
            if (type.Default != null)
                result.Add(type.Default);
            return result;
        }

        private ScopeContext GetImplementationsOfBaseType(string implementationReference, ImplementationContainer baseTypes)
        {
            if (implementationReference == TypeLocatorNames.DefaultName)
                return baseTypes.Default;
            ScopeContext context;
            return baseTypes.Typesets.TryGetValue(implementationReference, out context) ? context : null;
        }

        public void AddBaseTypeIfNonExisting(Type baseType)
        {
            Typesets.TryAdd(baseType.FullName, new ImplementationContainer());
        }

        public void AddImplementationTypeIfNonExisting(Type baseType, IScopeContext subType, string implementationReference)
        {
            var interfaceType = Typesets[baseType.FullName];
            if (implementationReference == TypeLocatorNames.DefaultName)
            {
                interfaceType.Default = (ScopeContext)subType;
                Defaults.TryAdd(baseType, (ScopeContext)subType);
                return;
            }
            if (!interfaceType.Typesets.ContainsKey(implementationReference))
                interfaceType.Typesets.TryAdd(implementationReference, (ScopeContext)subType);
        }

        public void RemoveImplementation(Type baseType, string implementationReference)
        {
            if (implementationReference == TypeLocatorNames.DefaultName)
            {
                Typesets[baseType.FullName].Default = null;
                ScopeContext item;
                Defaults.TryRemove(baseType, out item);
                return;
            }
            if (!Typesets[baseType.FullName].Typesets.ContainsKey(implementationReference)) return;
            if (implementationReference == TypeLocatorNames.DefaultName)
                Typesets[baseType.FullName].Default = null;
            else
            {
                ScopeContext removedItem;
                Typesets[baseType.FullName].Typesets.TryRemove(implementationReference, out removedItem);
            }
        }

        public void RemoveAll()
        {
            Typesets.Clear();
            Defaults.Clear();
        }

        //public ConcurrentDictionary<string, Dictionary<string, ScopeContext>> GetAllBindings()
        //{
        //    return Typesets;
        //}

        public IEnumerable<IScopeContext> GetAllSubClassesOf(Type baseType)
        {
            ImplementationContainer t;
            if (Typesets.TryGetValue(baseType.FullName, out t))
            {
                ScopeContext item;
                Defaults.TryRemove(baseType, out item);
                var results = (from i in t.Typesets.Values select i).ToList();
                if (t.Default != null)
                    results.Add(t.Default);
                return results;
            }
            return null;
        }
    }
}