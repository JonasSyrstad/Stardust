//
// mapfactory.cs
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
using System.ComponentModel;
using System.Diagnostics;
using Stardust.Wormhole.Converters;
using Stardust.Wormhole.MapBuilder;

namespace Stardust.Wormhole
{
    public static class MapFactory
    {
        static MapFactory()
        {
            Initialize();
        }

        public static void RegisterMappingDefinitions<T>() where T : IMappingDefinition, new()
        {
            var definitions = new T();
            definitions.Register();
        }

        public static long ElapsedTime { get; private set; }
        private static ConcurrentDictionary<KeyValuePair<Type, Type>, object> cache = new ConcurrentDictionary<KeyValuePair<Type, Type>, object>();
        private static bool Initialized;

        /// <summary>
        /// Creates a cached map instance. Initialize the mapping rule with custom mappings in type initializers or in app start. But only once in your application.
        /// </summary>
        /// <typeparam name="TInn">The type to convert from</typeparam>
        /// <typeparam name="TOut">The type to convert to</typeparam>
        /// <returns>a converted instance of the in instance</returns>
        public static IMapContainer<TInn, TOut> CreateMapRule<TInn, TOut>()
        {
            return CreateMapRule<TInn, TOut>(false);
        }

        /// <summary>
        /// Creates a cached map instance. Initialize the mapping rule with custom mappings in type initializers or in app start. But only once in your application.
        /// </summary>
        /// <typeparam name="TInn">The type to convert from</typeparam>
        /// <typeparam name="TOut">The type to convert to</typeparam>
        /// <returns>a converted instance of the in instance</returns>
        public static IMapContainer<TInn, TOut> CreateMapRule<TInn, TOut>(bool doMerge)
        {
            var timer = Stopwatch.StartNew();
            var key = new KeyValuePair<Type, Type>(typeof(TInn), typeof(TOut));
            object container;
            if (!cache.TryGetValue(key, out container))
            {
                var map = new MapContainer<TInn, TOut>();
                map.AddMap(doMerge);
                if (!cache.ContainsKey(key))
                    cache.TryAdd(key, map);
                ElapsedTime = timer.ElapsedMilliseconds;
                return map;
            }
            ElapsedTime = timer.ElapsedMilliseconds;
            return (IMapContainer<TInn, TOut>) container;
        }


        /// <summary>
        /// Creates empty map, this is not cached. Use this if you have separate mapping rules based on context. You need to cache this your self. Call AddMap to auto generate mapping details.
        /// </summary>
        /// <remarks>Creating new maps and running the mapping process the first time is expensive. Do not create new map for each conversion</remarks>
        /// <typeparam name="TInn">The type to convert from</typeparam>
        /// <typeparam name="TOut">The type to convert to</typeparam>
        /// <returns>a converted instance of the in instance</returns>
        public static IMapContainer<TInn, TOut> CreateEmptyMapRule<TInn, TOut>()
        {
            var map = new MapContainer<TInn, TOut>();
            return map;
        }

        internal static void Initialize()
        {
            if (!Initialized)
            {
                RegisterNewConverter(typeof(Type), typeof(TypeStringTypeConverter));
                ConverterFactory.Initialize();
            }
            Initialized = true;
        }

        /// <summary>
        /// Register a new type converter for type
        /// </summary>
        /// <param name="type">the type to convert</param>
        /// <param name="converterType">the type converter</param>
        public static void RegisterNewConverter(Type type, Type converterType)
        {
            TypeDescriptor.AddAttributes(type, new Attribute[] {new TypeConverterAttribute(converterType)});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TTarget">the type to convert</typeparam>
        /// <param name="converterType">the type converter</param>
        public static void RegisterNewConverter<TTarget>(Type converterType)
        {
            RegisterNewConverter(typeof(TTarget),converterType);
        }

        /// <summary>
        /// Register a new type converter for type
        /// </summary>
        /// <typeparam name="TTarget">the type to convert</typeparam>
        /// <typeparam name="TConverter">the type converter</typeparam>
        public static void RegisterNewConverter<TTarget,TConverter>()
        {
            RegisterNewConverter(typeof(TTarget), typeof(TConverter));
        }

        public static void ResetAllMaps()
        {
            cache.Clear();
        }

        /// <summary>
        /// Creates a map context for types
        /// </summary>
        /// <typeparam name="TInn">The type to map from</typeparam>
        /// <param name="source">The source instance</param>
        /// <returns></returns>
        public static ISimpleMapContext Map<TInn>(this TInn source) where TInn:class
        {
            return new SimpleMapContext<TInn>(source);
        }

        /// <summary>
        /// Creates a map context for IEnumerable of type T
        /// </summary>
        /// <typeparam name="TInn">The type to map from</typeparam>
        /// <param name="source">The source instance</param>
        /// <returns></returns>
        public static IListMapContext Map<TInn>(this IEnumerable<TInn> source)
        {
            return new ListMapContext<TInn>(source);
        }

        /// <summary>
        /// Creates a map context for IEnumerabes of type T
        /// </summary>
        /// <typeparam name="TInn">The type to map from</typeparam>
        /// <param name="source">The source instance</param>
        /// <returns></returns>
        public static IListMapContext Map<TInn>(this List<TInn> source)
        {
            return new ListMapContext<TInn>(source);
        }

        /// <summary>
        /// Creates a map context for dictionaries of  TInnKey,TInValue 
        /// </summary>
        /// <typeparam name="TInn">The type to map from</typeparam>
        /// <param name="source">The source instance</param>
        /// <returns></returns>
        public static IDictionaryMapContext Map<TInKey, TInValue>(this IDictionary<TInKey, TInValue> source)
        {

            return new DictionaryMapContext<TInKey, TInValue>(source);
        }
    }
}