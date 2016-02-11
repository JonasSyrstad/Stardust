//
// boundobjectinitializer.cs
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
using System.Collections.Generic;
using System.Reflection;
using Stardust.Nucleus.Internals;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;

namespace Stardust.Nucleus.ObjectActivator
{
    internal static class BoundObjectInitializer
    {
        private const BindingFlags BindingFlagsIncludePrivates = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private const BindingFlags BindingFlagsOnlyPublic = BindingFlags.Instance | BindingFlags.Public;
        private const BindingFlags FieldBindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField;
        private const BindingFlags PropertyBindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty;

        internal static object BindDependenciesCore(object instance)
        {
            if (!BindingCache.IsCached(instance))
                InstantiateByScanningType(instance);
            else
                InstantiateFromCache(instance);
            return instance;
        }

        private static void InstantiateFromCache(object instance)
        {
            foreach (var invocationInfo in BindingCache.GetInvocationInfo(instance))
                SetValue(instance, invocationInfo.Value);
        }

        private static void InstantiateByScanningType(object instance)
        {
            BindingCache.AddTypeToCache(instance);
            foreach (var member in GetMembers(instance))
            {
                InstantiateBoundElement(instance, member);
            }
        }

        private static void InstantiateBoundElement(object instance, MemberInfo member)
        {
            var attrib = member.GetCustomAttribute<BoundAttribute>(true);
            if (!attrib.IsInstance()) return;
            InvokeProperty(instance, member, attrib);
            InvokeField(instance, member, attrib);
        }

        private static void InvokeProperty(object instance, MemberInfo member, BoundAttribute boundAttribute)
        {
            if (member.MemberType != MemberTypes.Property) return;
            var propertyInfo = member as PropertyInfo;
            var type = propertyInfo.PropertyType;
            SetValue(instance, BindingCache.CacheMemberBinding(instance, member, boundAttribute, type, PropertyBindingFlags));
        }

        private static void InvokeField(object instance, MemberInfo member, BoundAttribute boundAttribute)
        {
            if (member.MemberType != MemberTypes.Field) return;
            var fieldInfo = member as FieldInfo;
            SetValue(instance, BindingCache.CacheMemberBinding(instance, member, boundAttribute, fieldInfo.FieldType, FieldBindingFlags));
        }

        private static void SetValue(object instance, InvocationInfo invocationInfo)
        {
            var boundTo = ResolveBoundTo(ref invocationInfo);
            instance.GetType().InvokeMember(invocationInfo.Name, invocationInfo.Binding, null, instance, new[] { boundTo.Activate(invocationInfo.Scope) });
        }

        private static Type ResolveBoundTo(ref InvocationInfo invocationInfo)
        {
            if (invocationInfo.BoundTo.IsNull())
                ResolveBoundType(ref invocationInfo);
            return invocationInfo.BoundTo;
        }

        private static void ResolveBoundType(ref InvocationInfo invocationInfo)
        {
            var boundType = (IScopeContextInternal) (invocationInfo.ResolverContext.IsNullOrWhiteSpace()
                ? Resolver.ConfigurationKernel.Resolve(invocationInfo.MemberType,TypeLocatorNames.DefaultName)
                : Resolver.ConfigurationKernel.Resolve(invocationInfo.MemberType, invocationInfo.ResolverContext));
            if (boundType.ActivationScope.HasValue)
                invocationInfo.Scope = boundType.ActivationScope.Value;
            invocationInfo.BoundTo = boundType.BoundType;
        }

        private static IEnumerable<MemberInfo> GetMembers(object instance)
        {
            return instance.GetType().GetMembers(
                BoundActivator.IncludePrivates
                    ? BindingFlagsIncludePrivates
                    : BindingFlagsOnlyPublic
                );
        }
        internal static T BindDependencies<T>(BoundActivationContext<T> instance)
        {
            if (instance.IsCached) return instance.Instance;
            return (T)BindDependenciesCore(instance.Instance);
        }
    }
}