//
// runtimehelper.cs
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Stardust.Particles
{
    public static class RuntimeHelper
    {
        private static ConcurrentDictionary<Type, ConcurrentDictionary<Type, Attribute>> TypeReflectionCache = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, Attribute>>();
        private static ConcurrentDictionary<MethodInfo, ConcurrentDictionary<Type, Attribute>> MethodReflectionCache = new ConcurrentDictionary<MethodInfo, ConcurrentDictionary<Type, Attribute>>();

        public static string GetBinFolderLocation()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
        }

        public static string GetProcessName()
        {
            var curentProcess = Process.GetCurrentProcess();
            return curentProcess.ProcessName;
        }

        public static long GetCurrentMemoryUsage()
        {
            var curentProcess = Process.GetCurrentProcess();
            return curentProcess.PrivateMemorySize64;
        }

        public static TimeSpan GetApplicationUptime()
        {
            var curentProcess = Process.GetCurrentProcess();
            return DateTime.Now - curentProcess.StartTime;
        }

        [ExcludeFromCodeCoverage]
        public static float GetTimeInGC()
        {
            return PerfCounter.GetTimeInGc()
                .NextValue();
        }

        [ExcludeFromCodeCoverage]
        public static float GetCurrentCpuUsage()
        {
            return PerfCounter.GetCPUCounter()
                .NextValue();
        }

        [ExcludeFromCodeCoverage]
        public static long GetLargeObjectHeapSize()
        {
            return PerfCounter.GetLargeObjectHeap()
                .NextSample()
                .RawValue;
        }

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
                root=new ConcurrentDictionary<Type, Attribute>();
                MethodReflectionCache.TryAdd(self ,root);
            }
            if (!root.ContainsKey(type)) root.AddOrUpdate(type, instance);
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
                    return (T) leaf;
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
            Cache(t,attribute.GetType(),attribute);
        }

        public static T GetAttribute<T>(this Type[] self) where T : Attribute
        {
            return self.Select(type => type.GetAttribute<T>()).FirstOrDefault(attrib => attrib.IsInstance());
        }

        private static void Cache<T>(Type self, Type type, T instance) where T : Attribute
        {
            if (!TypeReflectionCache.ContainsKey(self)) TypeReflectionCache.AddOrUpdate(self, new ConcurrentDictionary<Type, Attribute>());
            if (!TypeReflectionCache[self].ContainsKey(type)) TypeReflectionCache[self].AddOrUpdate(type, instance);
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
    }
}