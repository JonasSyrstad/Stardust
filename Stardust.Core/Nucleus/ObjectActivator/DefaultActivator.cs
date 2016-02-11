//
// defaultactivator.cs
// This file is part of Stardust
//
// Author: Jonas Syrstad (jsyrstad2+StardustCore@gmail.com), http://no.linkedin.com/in/jonassyrstad/) 
// Copyright (c) 2014 Jonas Syrstad. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Stardust.Nucleus.Internals;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;

namespace Stardust.Nucleus.ObjectActivator
{
    /// <summary>
    /// An activator implementation that does constructor injection
    /// </summary>
    internal class DefaultActivator : IActivator
    {
        public virtual T Activate<T>(Type type)
        {
            if (type.ContainsGenericParameters)
                return ActivateWithExternalGenerics<T>(type,typeof(T).GetGenericArguments());
            return (T) Activate(type);
        }

        public object ActivateWithExternalGenerics(Type type, Type[] genericParameters)
        {
            if (type.ContainsGenericParameters)
                type = type.MakeGenericType(genericParameters);
            var ctor = GetOrAddConstructorInfoFromCache(type);
            return ObjectFactory.CreateConstructor(ctor)();
        }

        public virtual object Activate(Type type)
        {
            if (type.ContainsGenericParameters)
                type = type.MakeGenericType(type.GetGenericArguments());
            var ctor = GetOrAddConstructorInfoFromCache(type);
            var args = FindConstructorParameters(type,ctor);
            return ObjectFactory.CreateConstructor(ctor)(args.ToArray());
        }

        private static ConstructorInfo GetOrAddConstructorInfoFromCache(Type module)
        {
            ConstructorInfo ctor;
            if (ConstructorCache.TryGetValue(module, out ctor)) return ctor;
            ctor = GetMostAppropriateConstructor(module);
            ConstructorCache.TryAdd(module, ctor);
            return ctor;
        }

        public T ActivateWithExternalGenerics<T>(Type type, Type[] genericParameters)
        {
            return (T) ActivateWithExternalGenerics(type, genericParameters);
        }

        private static ConstructorInfo GetMostAppropriateConstructor(Type module)
        {
            var ctor =
                (from c in module.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                 where c.GetCustomAttribute<UsingAttribute>().IsInstance()
                 select c).SingleOrDefault();
            if (ctor.IsNull())
                ctor = module.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).OrderBy(c => c.GetParameters().Length).FirstOrDefault();
            return ctor;
        }

        private static IEnumerable<object> FindConstructorParameters(Type module, ConstructorInfo ctor)
        {
            var constructorParameters =new List<object>();
            var scopeItems = new List<ScopeContext>();
            foreach (var methodAttribute in ctor.GetParameters())
            {
                var itemScopeContext = Resolver.ConfigurationKernel.Resolve(methodAttribute.ParameterType, TypeLocatorNames.DefaultName) as ScopeContext;
                scopeItems.Add(itemScopeContext);
                var instance = itemScopeContext.Activate();
                constructorParameters.Add(instance);
            }
            if (!diCache.ContainsKey(module))
                diCache.AddOrUpdate(module, scopeItems);
            return constructorParameters;
        }

        private static readonly ConcurrentDictionary<Type, ConstructorInfo> ConstructorCache = new ConcurrentDictionary<Type, ConstructorInfo>();
        private static ConcurrentDictionary<Type, List<ScopeContext>> diCache = new ConcurrentDictionary<Type, List<ScopeContext>>();
    }
}