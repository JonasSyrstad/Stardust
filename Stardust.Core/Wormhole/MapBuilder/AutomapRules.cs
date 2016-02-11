//
// automaprules.cs
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ServiceModel.Dispatcher;
using Stardust.Particles;
using Stardust.Wormhole.Converters;

namespace Stardust.Wormhole.MapBuilder
{
    internal class AutomapRules<TInn, TOut> : IAutomapRules<TInn, TOut>
    {
        private Func<object, object> CompiledGetExpression;
        public Dictionary<string, Func<object, object>> DottedExpressions { get;private set; }
        private Type[] OutGenericDefinition1;
        internal AutomapRules()
        {
            Fields = new List<KeyValuePair<string, IMapRules>>();
            DottedExpressions = new Dictionary<string, Func<object, object>>();
        }

        public bool BaseCachePrimed { get; set; }

        public BasicTypes BasicType { get; set; }

        public bool CachePrimed { get; set; }

        public Action<object, object> CompiledSetExpression { get; set; }

        public ITypeConverter Converter { get; set; }

        public bool Convertible { get; set; }

        public bool DottedSource { get; set; }

        public List<KeyValuePair<string, IMapRules>> Fields { get; internal set; }

        public Func<object, object> GetExpression
        {
            get { return CompiledGetExpression ?? (CompiledGetExpression = TypeExtensions.BuildGetAccessor(InMethodInfo)); }
        }

        public string InMemberName { get; set; }

        public MethodInfo InMethodInfo { get; set; }

        public PropertyInfo InPropertyInfo { get; set; }

        public Type InType { get; set; }

        public GetDelegate InValueDelegate { get; set; }

        public Type[] OutGenericDefinition
        {
            get
            {
                return OutGenericDefinition1 ?? (OutGenericDefinition1 = OutPropertyInfo.PropertyType.GetGenericArguments());
            }
            set { OutGenericDefinition1 = value; }
        }
        public string OutMemberName { get; set; }

        public MethodInfo OutMethodInfo { get; set; }
        public PropertyInfo OutPropertyInfo { get; set; }

        public Type OutType { get; set; }

        public IMapContainer Parent { get; set; }

        public IMapContainer<TInn, TOut> ParentTyped
        {
            get { return Parent as IMapContainer<TInn, TOut>; }
            internal set
            {
                Parent = value;
            }
        }

        public Action<object, object> SetExpression
        {
            get { return CompiledSetExpression ?? (CompiledSetExpression = TypeExtensions.BuildSetAccessor(OutMethodInfo)); }
        }

        public IAutomapRules<TInn, TOut> Add(string sourceField, string targetField, BasicTypes convertionType)
        {
            ValidateMappingRequest(sourceField, targetField);
            Fields.Add(new KeyValuePair<string, IMapRules>(sourceField,
                new MapRules
                {
                    BasicType = convertionType,
                    Convertible = convertionType == BasicTypes.Convertable || convertionType == BasicTypes.ComplexType,
                    OutMemberName = targetField,
                    InMemberName = sourceField,
                    Parent = Parent
                }));
            return this;
        }

        public IAutomapRules<TInn, TOut> Add<TInnProperty, TOutProperty>(Expression<Func<TInn, TInnProperty>> inPropertyLambda, Expression<Func<TOut, TOutProperty>> outPropertyLambda, BasicTypes convertionType = BasicTypes.Convertable)
        {
            dynamic inExpression = inPropertyLambda.Body;
            dynamic innName;
            if ((from c in inPropertyLambda.Body.ToString() where c == '.' select c).Count() > 1)
            {
                var name = inPropertyLambda.Body.ToString().Substring(inPropertyLambda.Body.ToString().IndexOf('.') + 1);
                innName = name;
            }
            else
                innName = inPropertyLambda.Body is MemberExpression ? inExpression.Member.Name : inExpression.Method.Name;
            dynamic outExpression = outPropertyLambda.Body;
            var outName = outPropertyLambda.Body is MemberExpression ? outExpression.Member.Name : outExpression.Method.Name;
            Add(innName, outName, convertionType);
            return this;
        }

        public Func<object, object> GetDotExpressionPart(string subItem, Type subType)
        {
            Func<object, object> func;
            if (!DottedExpressions.TryGetValue(subItem, out func))
            {
                var metod = subType.GetProperty(subItem, BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.NonPublic);
                func = TypeExtensions.BuildGetAccessor(metod.GetMethod);
                DottedExpressions.Add(subItem, func);
            }
            return func;
        }
        public PropertyInfo GetPropertyInfo<TProperty>(Expression<Func<TInn, TProperty>> propertyLambda)
        {
            var type = typeof(TInn);

            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a method, not a property.", propertyLambda));

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a field, not a property.", propertyLambda));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format("Expression '{0}' refers to a property that is not from type {1}.", propertyLambda, type));
            return propInfo;
        }

        public IAutomapRules<TI, TO> GetRule<TI, TO>()
        {
            return Parent.GetRule<TI, TO>();
        }
        public IAutomapRules<TInn, TOut> RemoveMapping(string sourceFieldToRemove)
        {
            var item = (from r in Fields where r.Key == sourceFieldToRemove select r).ToList();
            foreach (var keyValuePair in item)
            {
                Fields.Remove(keyValuePair);
            }
            return this;
        }

        private void ValidateMappingRequest(string sourceField, string targetField)
        {
            if ((from r in Fields where r.Key == sourceField && r.Value.OutMemberName == targetField select r).Any())
                throw new MultipleFilterMatchesException(string.Format("Multiple mappings added for {0} -> {1}", sourceField, targetField));
        }
    }
}