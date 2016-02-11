using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Stardust.Particles;

namespace Stardust.Nucleus.ObjectActivator
{
    /// <summary>
    /// Creates an object factory based on compiled lambda expressions
    /// </summary>
    public static class ObjectFactory
    {
        private static readonly Hashtable creatorCache = Hashtable.Synchronized(new Hashtable());
        private static readonly Hashtable constructorCache = Hashtable.Synchronized(new Hashtable());
        public delegate object DynamicStaticMethodConstructor(params object[] args);

        public static DynamicStaticMethodConstructor CreateConstructor(ConstructorInfo constructor)
        {
            if (constructor == null) throw new ArgumentNullException("constructor");
            var c = creatorCache[constructor] as DynamicStaticMethodConstructor;
            if (c.IsInstance()) return c;
            lock (creatorCache.SyncRoot)
            {
                var argsParameter = Expression.Parameter(typeof(object[]), "args");

                var constructorParams = constructor.GetParameters();
                Expression body = Expression.New(
                    constructor,
                    CreateParameterExpressions(constructorParams, argsParameter)
                    );
                var lambda = Expression.Lambda<DynamicStaticMethodConstructor>(
                    Expression.Convert(body, typeof(object)),
                    argsParameter
                    );
                c = creatorCache[constructor] as DynamicStaticMethodConstructor;
                if (c.IsInstance()) return c;
                var creator = lambda.Compile();
                creatorCache.Add(constructor, creator);
                return creator;
            }
        }

        private static Expression[] CreateParameterExpressions(ParameterInfo[] methodParams, ParameterExpression argsParameter)
        {
            return methodParams.Select(
                (parameter, index) =>
                Expression.Convert(
                    Expression.ArrayIndex(argsParameter, Expression.Constant(index)),
                    parameter.ParameterType
                )
            ).ToArray();
        }

        public static DynamicStaticMethodConstructor CreateConstructor(Type type)
        {
            return CreateConstructor(type, new object[0]);
        }
        public static DynamicStaticMethodConstructor CreateConstructor(Type type,object[] args)
        {
            var defaultCtor = FindConstructor(type, args);
            return CreateConstructor(defaultCtor);
        }

        /// <summary>
        /// Needs improvement
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static ConstructorInfo FindConstructor(Type type, object[] args)
        {
            var key = new KeyValuePair<Type, int>(type, args.Count());
            var defaultCtor = constructorCache[key] as ConstructorInfo;
            if (defaultCtor != null) return defaultCtor;
            lock (constructorCache)
            {
                var ctors = from ctor in
                    type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    orderby ctor.GetParameters().Count()
                    where ctor.GetParameters().Count()==args.Count()
                    select ctor;
                
                var c = ctors.First();
                defaultCtor = constructorCache[key] as ConstructorInfo;

                if (defaultCtor == null)
                    constructorCache.Add(key, c);
                defaultCtor = c;
            }
            return defaultCtor;
        }

        public static object Createinstance(Type type, object[] args)
        {
            return CreateConstructor(type, args)(args);
        }
    }
}