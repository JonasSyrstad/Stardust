using System;
using System.Collections.Concurrent;

namespace Stardust.Rest.Client
{
    public static class RestifyBuilder
    {
        private static Func<Type, object> activationHandler = type => Activator.CreateInstance(type);

        /// <summary>
        /// Add custom object factory code
        /// </summary>
        /// <param name="activatorFunc"></param>
        public static void SetActivator(Func<Type, object> activatorFunc)
        {
            activationHandler = activatorFunc;
        }
        private static readonly ConcurrentDictionary<string, Type> cache = new ConcurrentDictionary<string, Type>();
        public static Type BuildType<T>(Type baseType, Type sessionHandler)
        {
            Type proxytype;
            if (cache.TryGetValue(typeof(T).FullName, out proxytype)) return proxytype;
            proxytype = new RestifyBuilder<T>(baseType, sessionHandler).CreateRestProxy();
            cache.TryAdd(typeof(T).FullName, proxytype);
            return proxytype;
        }

        public static T CreateProxy<T>(Type baseType, Type sessionHandler)
        {

            return (T)activationHandler(BuildType<T>(baseType, sessionHandler));
        }

    }
}