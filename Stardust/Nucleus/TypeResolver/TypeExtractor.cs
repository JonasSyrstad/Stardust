//
// typeextractor.cs
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stardust.Nucleus.Extensions;
using Stardust.Nucleus.Internals;
using Stardust.Particles;

namespace Stardust.Nucleus.TypeResolver
{
    public static class TypeExtractor
    {
        private const string IncorrectVersionOfTheAssemblyFound = "Incorrect version of the assembly; found \"{0}\", expected: \"{1}\"";

        public static IScopeContext LoadTypeFromFile(ObjectInitializer implementationReference)
        {
            var assembly = Assembly.LoadFile(implementationReference.AssemblyPath);
            if (assembly.IsIncorrectVersion(implementationReference))
                throw new ModuleCreatorException(String.Format(IncorrectVersionOfTheAssemblyFound, assembly.FullName, implementationReference.AssemblyName));
            var module = assembly.GetType(implementationReference.FullName);
            return module.ConvertToContext();
        }

        public static IEnumerable<Type> RetreiveSubTypesFromAssembly(Type baseType, Assembly assembly)
        {
            if (baseType.IsInterface)
                return RetrieveTypesFromAssemblyThatImplements(baseType, assembly);
            return RetrieveTypesFromAssemblyThatExstends(baseType, assembly);
        }

        internal static ConcurrentBag<Type> GetAllSubClassesOf(Type baseType)
        {
            Logging.DebugMessage(string.Format("Assembly scan initiated for type {0}. To improve warm up performance add a binding in config or in code.",baseType.FullName),EventLogEntryType.Warning);
            var typeList = new ConcurrentBag<Type>();
            try
            {
                Parallel.ForEach(GetAssemblies(), assembly =>
                                     {
                                         var types = RetreiveSubTypesFromAssembly(baseType, assembly);
                                         AddTypesToTypeList(types, typeList);
                                     });
            }
            catch (Exception ex)
            {
                throw new ModuleCreatorException(string.Format("Cannot extract type {0} from loaded assemblies", baseType.FullName), ex);
            }
            return typeList;
        }

        internal static IEnumerable<IScopeContext> GetClassList(ObjectInitializer implementationReference, Type ofType)
        {
            IEnumerable<Type> items;
            if (implementationReference.AssemblyName.IsNullOrEmpty())
            {
                items = GetAllSubClassesOf(ofType);
            }
            else
                items = RetreiveSubTypesFromAssembly(ofType, Assembly.Load(implementationReference.AssemblyName)).ToList();
            return (from i in items select i.ConvertToContext()).ToList();
        }

        internal static ScopeContext GetTypeFromFullName(ObjectInitializer implementationReference)
        {
            if (implementationReference.ModuleType.IsInstance()) return implementationReference.ModuleType.ConvertToContext().SetScope(implementationReference.Scope);
            var assembly = Assembly.Load(implementationReference.AssemblyName);
            var module = assembly.GetType(implementationReference.FullName).ConvertToContext().SetScope(implementationReference.Scope);
            return module;
        }

        private static void AddTypesToTypeList(IEnumerable<Type> types, ConcurrentBag<Type> typeList)
        {
            if (types.IsInstance())
                typeList.AddRange(types);
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            return from assembly in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
                   where ShouldIncludeAssemblyInSearch(assembly)
                   select assembly;
        }

        private static bool ImplementsInterfaceAndIsConcreteType(Type interfaceType, Type type)
        {
            try
            {
                return CheckAndValidateParent(interfaceType, type)
                        && !type.IsAbstract;
            }
            catch (Exception ex)
            {

                throw new ModuleCreatorException(string.Format("Cannot determine if {1} is concrete and of type {0}.", interfaceType.FullName, type.FullName), ex);
            }
        }

        private static bool CheckAndValidateParent(Type interfaceType, Type type)
        {
            var ifType = type.GetInterface(interfaceType.Name, true);
            if (ifType.IsNull()) return false;
            return ifType.FullName == interfaceType.FullName;
        }

        private static bool IsIncorrectVersion(this Assembly self, ObjectInitializer implementationReference)
        {
            if (implementationReference.AssemblyName.Contains("version"))
                return self.FullName.EqualsCaseInsensitive(implementationReference.AssemblyName);
            return self.GetName().Name.EqualsCaseInsensitive(implementationReference.AssemblyName);
        }

        private static bool IsSubTypeOfAndIsConcreteType(Type baseClass, Type type)
        {
            return type.IsSubclassOf(baseClass)
                && !type.IsAbstract;
        }

        private static IEnumerable<Type> RetrieveTypesFromAssemblyThatExstends(Type baseClass, Assembly assembly)
        {
            var types = from type in assembly.GetTypes().AsParallel()
                        where IsSubTypeOfAndIsConcreteType(baseClass, type)
                        select type;
            return types;
        }

        private static IEnumerable<Type> RetrieveTypesFromAssemblyThatImplements(Type interfaceType, Assembly assembly)
        {
            try
            {
                var types = from type in assembly.GetTypes().AsParallel()
                            where ImplementsInterfaceAndIsConcreteType(interfaceType, type)
                            select type;
                return types;
            }
            catch (Exception ex)
            {
                Logging.Exception(ex, interfaceType.Name);
                throw new ModuleCreatorException("Unexpected error occured while loading " + interfaceType.Name, ex);
            }
        }

        private static bool ShouldIncludeAssemblyInSearch(Assembly assembly)
        {
            return !assembly.FullName.StartsWithOneOfCaseInsensitive("system", "Microsoft", "log4net", "mscorlib", "dynamicproxy");
        }
    }
}