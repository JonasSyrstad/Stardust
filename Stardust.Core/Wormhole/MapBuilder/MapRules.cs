//
// maprules.cs
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
using Stardust.Particles;
using Stardust.Wormhole.Converters;

namespace Stardust.Wormhole.MapBuilder
{
    internal class MapRules : IMapRules
    {
        public MapRules()
        {
            DottedExpressions=new Dictionary<string, Func<object, object>>();
        }

        private Type[] OutGenericDefinition1;
        private Func<object, object> CompiledGetExpression;
        private Action<object, object> CompiledSetExpression;
        public Dictionary<string, Func<object, object>> DottedExpressions { get; private set; }
        public string OutMemberName { get; set; }
        public bool Convertible { get; set; }
        public PropertyInfo InPropertyInfo { get; set; }
        public PropertyInfo OutPropertyInfo { get; set; }

        public BasicTypes BasicType { get; set; }
        public IMapContainer Parent { get;  set; }
        public string InMemberName { get; set; }

        public Func<object, object> GetExpression
        {
            get { return CompiledGetExpression ?? (CompiledGetExpression = TypeExtensions.BuildGetAccessor(InMethodInfo)); }
        }

        public Action<object, object> SetExpression
        {
            get { return CompiledSetExpression ?? (CompiledSetExpression = TypeExtensions.BuildSetAccessor(OutMethodInfo)); }
        }

        public ITypeConverter Converter { get; set; }

        public Type[] OutGenericDefinition
        {
            get
            {
                if (OutGenericDefinition1 == null)
                    OutGenericDefinition1 = OutPropertyInfo.PropertyType.GetGenericArguments();
                return OutGenericDefinition1;
            }
            set { OutGenericDefinition1 = value; }
        }

        public bool CachePrimed { get; set; }
        public bool BaseCachePrimed { get; set; }
        public MethodInfo InMethodInfo { get; set; }
        public MethodInfo OutMethodInfo { get; set; }
        public bool DottedSource { get; set; }
        public Func<object, object> GetDotExpressionPart(string subItem, Type subType)
        {
            Func<object, object> func;
            if (!DottedExpressions.TryGetValue(subItem, out func))
            {
                var metod = subType.GetProperty(subItem, BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
                func = TypeExtensions.BuildGetAccessor(metod.GetMethod);
                DottedExpressions.Add(subItem, func);
            }
            return func;
        }

        public Type OutType { get; set; }
        public Type InType { get; set; }
        public GetDelegate InValueDelegate { get; set; }
    }
}