//
// mapcontainer.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Stardust.Nucleus.Extensions;
using Stardust.Nucleus.ObjectActivator;
using Stardust.Particles;

namespace Stardust.Wormhole.MapBuilder
{
    internal class MapContainer<TInn, TOut> : IMapContainer<TInn, TOut>
    {
        private static Type AmapDef;
        private static object TrioWing = new object();
        private readonly DictionaryMapper<TInn, TOut> DictionaryMapper;
        private readonly ListMapper<TInn, TOut> ListMapper;
        private readonly KeyValuePair<Type, Type> RuleKey;
        private readonly TypeMapper TypeMapper;
        internal MapContainer()
        {
            MapFactory.Initialize();
            RuleKey = new KeyValuePair<Type, Type>(typeof(TInn), typeof(TOut));
            Rules = new Dictionary<KeyValuePair<Type, Type>, IAutomapRules>();
            ListMapper = new ListMapper<TInn, TOut>(this);
            DictionaryMapper = new DictionaryMapper<TInn, TOut>(this);
            TypeMapper = new TypeMapper(this);
        }

        public Dictionary<KeyValuePair<Type, Type>, IAutomapRules> Rules { get; private set; }

        private static Type GenericAutomapRuleDefinition
        {
            get
            {
                if (AmapDef == null)
                {
                    lock (TrioWing)
                    {
                        if (AmapDef == null)
                            AmapDef = typeof(AutomapRules<object, object>).GetGenericTypeDefinition();
                    }
                }
                return AmapDef;
            }
        }

        public IAutomapRules<TInn, TOut> Add<TInnProperty, TOutProperty>(Expression<Func<TInn, TInnProperty>> inPropertyLambda, Expression<Func<TOut, TOutProperty>> outPropertyLambda, BasicTypes convertionType = BasicTypes.Convertable)
        {
            return GetRule<TInn, TOut>().Add(inPropertyLambda, outPropertyLambda, convertionType);
        }

        public IAutomapRules<TInn, TOut> Add(string sourceField, string targetField, BasicTypes convertionType)
        {
            return GetRule<TInn, TOut>().Add(sourceField, targetField, convertionType);
        }

        public IAutomapRules AddMap<TI, TO>(Dictionary<string, string> simpleMap)
        {
            return AddMap(typeof(TI), typeof(TO), simpleMap);
        }

        public IAutomapRules AddMap<TI, TO>()
        {
            return AddMap(typeof(TI), typeof(TO));
        }

        public IAutomapRules AddMap(Type innType, Type outType, Dictionary<string, string> simpleMap)
        {
            return AddMap(innType, outType, simpleMap, false);
        }
        public IAutomapRules AddMap(Type innType, Type outType, Dictionary<string, string> simpleMap, bool doMerge)
        {
            var key = new KeyValuePair<Type, Type>(innType, outType);
            var rule = ActivatorFactory.Activator.ActivateWithExternalGenerics<IAutomapRules>(GenericAutomapRuleDefinition, new[] { innType, outType });
            rule.InType = innType;
            rule.OutType = outType;
            //rule.Fields = new List<KeyValuePair<string, IMapRules>>();
            rule.Convertible = !doMerge;
            rule.Parent = this;
            rule.BasicType = doMerge ? BasicTypes.ComplexType : MapHelper.GetConversionType(innType, outType);
            Rules.Add(key, rule);
            CreateMapingDefinition(simpleMap, rule);
            return rule;
        }

        public IAutomapRules AddMap(Type innType, Type outType)
        {
            return AddMap(innType, outType, null);
        }

        public void AddMap()
        {
            AddMap(typeof(TInn), typeof(TOut));
        }

        public bool ContainsRule(KeyValuePair<Type, Type> keyValuePair)
        {
            return Rules.ContainsKey(keyValuePair);
        }

        public void ExtractMappingDefinition(IAutomapRules map)
        {
            var innType = map.InType.GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
            var outType = map.OutType.GetProperties(BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.Public);
            GetMappingInfo(innType, outType, map);
        }

        public KeyValuePair<Type, Type> GetKey()
        {
            return RuleKey;
        }
        public IAutomapRules<TI, TO> GetRule<TI, TO>()
        {
            var key = new KeyValuePair<Type, Type>(typeof(TI), typeof(TO));
            IAutomapRules rule;
            if (Rules.TryGetValue(key, out rule))
                return (IAutomapRules<TI, TO>)rule;
            rule = AddMap<TI, TO>();
            return (IAutomapRules<TI, TO>)rule;
        }

        public IAutomapRules<TInn, TOut> GetRule()
        {
            return (IAutomapRules<TInn, TOut>)GetRule(typeof(TInn), typeof(TOut), false);
        }
        public IAutomapRules<TInn, TOut> GetRule(bool doMerge)
        {
            return (IAutomapRules<TInn, TOut>)GetRule(typeof(TInn), typeof(TOut), doMerge);
        }

        public IAutomapRules GetRule(Type innType, Type outType)
        {
            return GetRule(innType, outType, false);
        }

        public IAutomapRules GetRule(Type innType, Type outType, bool doMerge)
        {
            var key = new KeyValuePair<Type, Type>(innType, outType);
            IAutomapRules rule;
            Rules.TryGetValue(key, out rule);
            if (rule != null) return rule;
            if (!TypeExtensions.CanConvert(outType, innType))
            {
                throw new ArgumentOutOfRangeException(string.Format("Cannot find mapping for type{0}->{1}", innType, outType));
            }
            return new AutomapRules<object, object> { BasicType = doMerge ? BasicTypes.ComplexType : BasicTypes.Convertable, Parent = this, OutType = outType, InType = innType };
        }

        private void CreateMapingDefinition(Dictionary<string, string> simpleMap, IAutomapRules rule)
        {
            if (simpleMap.IsInstance())
            {
                foreach (var mapItem in simpleMap)
                {
                    rule.Fields.Add(new KeyValuePair<string, IMapRules>(mapItem.Key,
                        new MapRules
                        {
                            InMemberName = mapItem.Key,
                            Convertible = true,
                            OutMemberName = mapItem.Value,
                            BasicType = BasicTypes.Convertable
                        }));
                }
                rule.BasicType = BasicTypes.ComplexType;
                rule.Convertible = false;
            }
            else
            {
                ExtractMappingDefinition(rule);
            }
        }
        private bool GetFieldMappingInfo(IAutomapRules map, PropertyInfo outPropertyInfo, PropertyInfo innPropertyInfo)
        {

            if (!CanMapField(outPropertyInfo, innPropertyInfo)) return false;
            if (outPropertyInfo.PropertyType.Implements(typeof(IDictionary)))
            {
                DictionaryMapper.MapDictionary(map, innPropertyInfo, outPropertyInfo);
            }
            else if (outPropertyInfo.PropertyType.Implements(typeof(IEnumerable)) &&
                     !(outPropertyInfo.PropertyType == typeof(string)))
            {
                ListMapper.MapIEnumerable(map, innPropertyInfo, outPropertyInfo);
            }
            else
            {
                TypeMapper.MapType(map, innPropertyInfo, outPropertyInfo);
            }
            return true;
        }

        private static bool CanMapField(PropertyInfo outPropertyInfo, PropertyInfo innPropertyInfo)
        {
            if (!outPropertyInfo.Name.Equals(innPropertyInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                return false;
            if (outPropertyInfo.SetMethod == null) return false;
            if (outPropertyInfo.SetMethod.IsPrivate) return false;
            if (innPropertyInfo.GetMethod == null) return false;
            if (innPropertyInfo.GetMethod.IsPrivate) return false;
            return true;
        }

        private void GetMappingInfo(IEnumerable<PropertyInfo> innType, PropertyInfo[] outType, IAutomapRules map)
        {
            foreach (var innPropertyInfo in innType)
            {
                foreach (var outPropertyInfo in outType)
                {
                    if (GetFieldMappingInfo(map, outPropertyInfo, innPropertyInfo)) break;
                }
            }
        }

        public void AddMap(bool doMerge)
        {
            AddMap(typeof(TInn), typeof(TOut), null, doMerge);
        }
    }
}