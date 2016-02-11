//
// advancedactivator.cs
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
using Stardust.Nucleus.Extensions;
using Stardust.Nucleus.Internals;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;
using Stardust.Particles.Collection.Arrays;

namespace Stardust.Nucleus.ObjectActivator
{
    /// <summary>
    /// Uses different activation patterns to increase performance. 
    /// </summary>
    /// <typeparam name="T">The type of instance to create</typeparam>
    /// <remarks>Since this is used as a part of the IoC container it is typed with T but requires a type that inherits from T or is T it self.</remarks>
    internal sealed class AdvancedActivator<T>
    {
        private const string InvalidModuleErrorMessage = "Invalid module. Please provide a valid type that inherits from {0}";
        private const string IsAGenericTypeErrorMessage = "\"{0}\" is a generic type, please provide the inner type";
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> ConstructorCache = new ConcurrentDictionary<Type, ConstructorInfo>();
        private static ConcurrentDictionary<Type, List<ScopeContext>> diCache = new ConcurrentDictionary<Type, List<ScopeContext>>();
        private readonly Type[] InnerTypes;


        /// <summary>
        /// Creates an instance of the advanced activator
        /// </summary>
        public AdvancedActivator()
        {
            var type = typeof(T);
            InnerTypes = type.IsGenericType ? type.GetGenericArguments() : new Type[0];
            ValidateActivatorType();
        }

        /// <summary>
        /// Creates an instance of the advanced activator
        /// </summary>
        /// <param name="innerTypes">provides information about the generic type parameters</param>
        public AdvancedActivator(Type[] innerTypes)
        {
            InnerTypes = innerTypes;
            ValidateActivatorType();
        }

        /// <summary>
        /// Creates an instance of type module that implements T or is T
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        /// <exception cref="ModuleCreatorException"></exception>
        public T CreateInstance(Type module)
        {
            if (IsInvalidModule(module))
                throw (new ModuleCreatorException(String.Format(InvalidModuleErrorMessage, typeof(T).Name)));
            if (module.ContainsGenericParameters)
                return CreateGenericInstanceOf(module);
            return CreateInstanceOf(module);
        }

        public T CreateInstance(Type module, object[] args)
        {
            if (module.ContainsGenericParameters)
            {
                module = module.MakeGenericType(InnerTypes);
            }
            if (ArrayExtensions.IsEmpty(args))
                return (T)ObjectFactory.CreateConstructor(module)();
            return (T)ObjectFactory.CreateConstructor(module, args)(args);
        }

        private static T CreateInstanceOf(Type module)
        {

            var ctor = GetOrAddConstructorInfoFromCache(module);
            if (ctor.IsInstance())
            {
                var constructorParameters = GetConstructorParameters(module, ctor);
                return (T)ObjectFactory.CreateConstructor(ctor)(constructorParameters);
            }
            return (T)Activator.CreateInstance(module, true);
        }

        private static void FindCachedConstructorItems(Type module, ICollection<object> constructorParameters)
        {
            foreach (var items in diCache[module])
                constructorParameters.Add(items.Activate());
        }

        private static void FindConstructorParameters(Type module, ConstructorInfo ctor, ICollection<object> constructorParameters)
        {
            var scopeItems = new List<ScopeContext>();
            foreach (var methodAttribute in ctor.GetParameters())
            {
                var itemScopeContext = Resolver.ConfigurationKernel.Resolve(methodAttribute.ParameterType,TypeLocatorNames.DefaultName) as ScopeContext;
                scopeItems.Add(itemScopeContext);
                var instance = itemScopeContext.Activate();
                constructorParameters.Add(instance);
            }
            if (!diCache.ContainsKey(module))
                diCache.AddOrUpdate(module, scopeItems);
        }

        private static object[] GetConstructorParameters(Type module, ConstructorInfo ctor)
        {
            var constructorParameters = new List<object>();
            if (diCache.ContainsKey(module))
                FindCachedConstructorItems(module, constructorParameters);
            else
                FindConstructorParameters(module, ctor, constructorParameters);
            return constructorParameters.ToArray();
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

        private static ConstructorInfo GetOrAddConstructorInfoFromCache(Type module)
        {
            ConstructorInfo ctor;
            if (ConstructorCache.TryGetValue(module, out ctor)) return ctor;
            ctor = GetMostAppropriateConstructor(module);
            ConstructorCache.TryAdd(module, ctor);
            return ctor;
        }

        private static bool IsInvalidModule(Type module)
        {
            if (module.IsNull())
                return true;
            return !module.Implements(typeof(T));
        }

        private Type ConstructConcreteTypeForGeneric(Type module)
        {
            var genericType = module.MakeGenericType(InnerTypes);
            return genericType;
        }

        private T CreateGenericInstanceOf(Type module)
        {
            var genericType = ConstructConcreteTypeForGeneric(module);
            var typeInstance = CreateInstanceOf(genericType);
            return typeInstance;
        }

        private bool HasGenericParametersAndInnerTypeFunctionNotDefined()
        {
            return ArrayExtensions.IsEmpty(InnerTypes)
                && typeof(T).IsGenericType;
        }

        private bool IsInnertypeFunctionInvalid()
        {
            if (!typeof(T).IsGenericType)
                return false;
            return InnerTypes.Any(innerType => innerType.IsGenericType);
        }

        private void ValidateActivatorType()
        {
            if (HasGenericParametersAndInnerTypeFunctionNotDefined())
                throw new ModuleCreatorException(String.Format(IsAGenericTypeErrorMessage, typeof(T).Name));
            if (IsInnertypeFunctionInvalid())
                throw new ModuleCreatorException(String.Format(IsAGenericTypeErrorMessage, typeof(T).Name));
        }
    }
}