//
// bindingcache.cs
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
using Stardust.Particles;

namespace Stardust.Nucleus.ObjectActivator
{
    internal static class BindingCache
    {
        private static readonly Dictionary<Type, Dictionary<string, InvocationInfo>> InvocationCache = new Dictionary<Type, Dictionary<string, InvocationInfo>>();

        internal static InvocationInfo CacheMemberBinding(object instance, MemberInfo member, BoundAttribute boundAttribute, Type type, BindingFlags bindingFlags)
        {
            var cache = InvocationCache[instance.GetType()];
            InvocationInfo info;

            if (!cache.TryGetValue(member.Name,out info))
            {
                lock (InvocationCache)
                {
                    var invocationInfo = CreateInvocationInfo(member, boundAttribute, type, bindingFlags);
                    if (!cache.TryGetValue(member.Name, out info))
                        cache.Add(member.Name, invocationInfo);
                    return invocationInfo;
                }
            }
            return info;
        }

        internal static bool IsCached(object instance)
        {
            return InvocationCache.ContainsKey(instance.GetType());
        }

        private static InvocationInfo CreateInvocationInfo(MemberInfo member, BoundAttribute boundAttribute, Type type, BindingFlags bindingFlags)
        {
            return new InvocationInfo
                       {
                           Name = member.Name,
                           BoundTo = boundAttribute.BoundTo,
                           Scope = boundAttribute.ScopeContext,
                           MemberInfo = member,
                           MemberType = type,
                           Binding = bindingFlags,
                           ResolverContext = boundAttribute.ResolverContext,
                           SetExpression=TypeExtensions.BuildSetAccessor(member)
                       };
        }

        internal static void AddTypeToCache(object instance)
        {
            lock (InvocationCache)
            {
                if (!InvocationCache.ContainsKey(instance.GetType()))
                    InvocationCache.Add(instance.GetType(), new Dictionary<string, InvocationInfo>());
            }
        }

        internal static Dictionary<string, InvocationInfo> GetInvocationInfo(object instance)
        {
            return InvocationCache[instance.GetType()];
        }
    }
}