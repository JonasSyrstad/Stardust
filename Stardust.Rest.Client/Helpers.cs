using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Stardust.Rest.Client
{
    internal static class Helpers
    {
        public static T GetAttribute<T>(this MethodInfo self) where T : Attribute
        {
            bool hasItem;
            var instance = LookupInCache<T>(self, typeof(T), out hasItem);
            if (hasItem) return instance;
            var attrib = from a in self.GetCustomAttributes(true)
                         where a is T
                         select a;
            instance = (T)attrib.FirstOrDefault();
            Cache(self, typeof(T), instance);
            return instance;
        }

        private static void Cache<T>(MethodInfo self, Type type, T instance) where T : Attribute
        {

            ConcurrentDictionary<Type, Attribute> root;
            if (!MethodReflectionCache.TryGetValue(self, out root))
            {
                root = new ConcurrentDictionary<Type, Attribute>();
                MethodReflectionCache.TryAdd(self, root);
            }
            if (!root.ContainsKey(type)) root.TryAdd(type, instance);
        }

        private static T LookupInCache<T>(MethodInfo self, Type type, out bool hasItem) where T : Attribute
        {
            ConcurrentDictionary<Type, Attribute> root;
            if (MethodReflectionCache.TryGetValue(self, out root))
            {
                Attribute leaf;
                if (root.TryGetValue(type, out leaf))
                {
                    hasItem = true;
                    return (T)leaf;
                }
            }
            hasItem = false;
            return default(T);
        }

        public static T GetAttribute<T>(this Type self) where T : Attribute
        {

            bool hasItem;
            var instance = LookupInCache<T>(self, typeof(T), out hasItem);
            if (hasItem) return instance;
            var attrib = from a in self.GetCustomAttributes(true)
                         where a is T
                         select a;
            instance = (T)attrib.FirstOrDefault();
            Cache(self, typeof(T), instance);
            return instance;

        }

        public static void AddAttribute<T>(Attribute attribute)
        {
            var t = typeof(T);
            Cache(t, attribute.GetType(), attribute);
        }

        public static T GetAttribute<T>(this Type[] self) where T : Attribute
        {
            return self.Select(type => type.GetAttribute<T>()).FirstOrDefault(attrib => attrib != null);
        }

        private static void Cache<T>(Type self, Type type, T instance) where T : Attribute
        {
            if (!TypeReflectionCache.ContainsKey(self)) TypeReflectionCache.TryAdd(self, new ConcurrentDictionary<Type, Attribute>());
            if (!TypeReflectionCache[self].ContainsKey(type)) TypeReflectionCache[self].TryAdd(type, instance);
        }

        private static T LookupInCache<T>(Type self, Type type, out bool hasItem) where T : Attribute
        {
            ConcurrentDictionary<Type, Attribute> root;
            if (TypeReflectionCache.TryGetValue(self, out root))
            {
                Attribute leaf;
                if (root.TryGetValue(type, out leaf))
                {
                    hasItem = true;
                    return (T)leaf;
                }
            }
            hasItem = false;
            return default(T);


        }

        private static ConcurrentDictionary<Type, ConcurrentDictionary<Type, Attribute>> TypeReflectionCache = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, Attribute>>();
        private static ConcurrentDictionary<MethodInfo, ConcurrentDictionary<Type, Attribute>> MethodReflectionCache = new ConcurrentDictionary<MethodInfo, ConcurrentDictionary<Type, Attribute>>();

    }
}