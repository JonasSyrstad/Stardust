//
// simpletypeconverter.cs
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
using Stardust.Nucleus.ObjectActivator;
using Stardust.Particles;
using Stardust.Wormhole.MapBuilder;

namespace Stardust.Wormhole.Converters
{
     class ExpanderTypeConverter : SimpleTypeConverter
    {
        protected override object DoConvert(object source, object target = null)
        {
            var intype = GetInnType(source);
            var outtype = Rule.OutMethodInfo.ReturnType;
            var newTarget = ActivatorFactory.Activator.Activate(OutType());
            return base.DoConvert(source, newTarget);
            return null;
        }
    }
    class SimpleTypeConverter : TypeConverterBase
    {
        protected override object DoConvert(object source, object target = null)
        {
            var newtarget = target ?? ActivatorFactory.Activator.Activate(OutType());
            foreach (var mapRules in Rule.Parent.GetRule(GetInnType(source), OutType()).Fields)
                ConvertProperty(source, mapRules, newtarget);
            return newtarget;
        }

        private void ConvertProperty(object source, KeyValuePair<string, IMapRules> mapRules, object newtarget)
        {
            try
            {   
                EnsurePropertyInfoCache(source, mapRules);
                var outPropValue = ConverterFactory.Convert(mapRules, GetInValue(source, mapRules),this.UpdateParentTarget(mapRules)? newtarget:null);
                if (mapRules.Value.OutPropertyInfo.IsInstance() && outPropValue != null)
                    mapRules.Value.SetExpression(newtarget, outPropValue);
            }
            catch (Exception ex)
            {

                throw new InvalidCastException(string.Format("Unable to convert {0} to {1}", mapRules.Key, mapRules.Value.OutMemberName), ex);
            }
        }

        private bool UpdateParentTarget(KeyValuePair<string, IMapRules> mapRules)
        {
            return (this.InType()==this.OutType()||mapRules.Value.BasicType==BasicTypes.FlatenComplexType);
        }

        private static object GetInValue(object source, KeyValuePair<string, IMapRules> mapRules)
        {

            if (mapRules.Value.BasicType != BasicTypes.Expander)
                return GetValueDotted(source, mapRules);
            return source;
        }

        private static object GetValueDotted(object source, KeyValuePair<string, IMapRules> mapRules)
        {
            if (!HasDottedSource(mapRules)) return mapRules.Value.GetExpression(source);
            return getDottedValueFromSource(source, mapRules);
        }

        private static object getDottedValueFromSource(object source, KeyValuePair<string, IMapRules> mapRules)
        {
            var value = source;
            if (mapRules.Value.DottedSource)
            {
                foreach (var expression in mapRules.Value.DottedExpressions)
                {
                    value = expression.Value(value);
                    if (value == null)
                        return null;
                }
                return value;
            }
            var subItems = mapRules.Key.Split('.');
            foreach (var subItem in subItems)
            {
                if (value != null)
                    mapRules.Value.InPropertyInfo = value.GetType().GetProperty(subItem, GetInnBindingFlags());
                if (value == null) return null;
                value = mapRules.Value.GetDotExpressionPart(subItem, value.GetType())(value);
            }
            mapRules.Value.DottedSource = true;
            return value;
        }

        private static bool HasDottedSource(KeyValuePair<string, IMapRules> mapRules)
        {
            return mapRules.Value.DottedSource || mapRules.Key.Contains(".");
        }

        protected void EnsurePropertyInfoCache(object source, KeyValuePair<string, IMapRules> mapRules)
        {
            if (mapRules.Value.CachePrimed) return;
            if (mapRules.Value.InPropertyInfo == null && !mapRules.Key.Contains("."))
            {
                mapRules.Value.InPropertyInfo = GetInnType(source).GetProperty(mapRules.Key, GetInnBindingFlags());
                mapRules.Value.InMethodInfo = mapRules.Value.InPropertyInfo.GetMethod;

            }
            if (mapRules.Value.OutMemberName == "this")
            {
                mapRules.Value.CachePrimed = true;
                return;
            }
            if (mapRules.Value.OutPropertyInfo == null)
            {
                mapRules.Value.OutPropertyInfo = OutType().GetProperty(mapRules.Value.OutMemberName, GetOutBindingFlags());
                mapRules.Value.OutMethodInfo = mapRules.Value.OutPropertyInfo.SetMethod;
            }
            mapRules.Value.CachePrimed = true;
        }

        protected Type GetInnType(object source)
        {
            return Rule.BasicType == BasicTypes.Expander ? source.GetType() : InType();
        }
    }
}