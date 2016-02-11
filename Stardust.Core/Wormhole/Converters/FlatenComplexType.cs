//
// flatencomplextype.cs
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
using Stardust.Wormhole.MapBuilder;

namespace Stardust.Wormhole.Converters
{
    class FlatenComplexType : TypeConverterBase
    {
        protected override object DoConvert(object source, object target = null)
        {
            foreach (var mapRules in Rule.Parent.GetRule(source.GetType(), target.GetType()).Fields)
            {
                EnsurePropertyInfoCache(mapRules, source.GetType(), target.GetType());
                var innPropValue = mapRules.Value.GetExpression(source);
                var outPropValue = ConverterFactory.Convert(mapRules, innPropValue);
                mapRules.Value.SetExpression(target, outPropValue);
            }
            return target;
        }

        private void EnsurePropertyInfoCache(KeyValuePair<string, IMapRules> mapRules, Type inType, Type ouType)
        {
            if (mapRules.Value.CachePrimed) return;
            if (mapRules.Value.InPropertyInfo == null)
            {
                mapRules.Value.InPropertyInfo = inType.GetProperty(mapRules.Key, GetInnBindingFlags());
                mapRules.Value.InMethodInfo = mapRules.Value.InPropertyInfo.GetMethod;
            }
            if (mapRules.Value.OutMemberName == "this")
            {
                mapRules.Value.CachePrimed = true;
                return;
            }
            if (mapRules.Value.OutPropertyInfo == null)
            {
                mapRules.Value.OutPropertyInfo = ouType.GetProperty(mapRules.Value.OutMemberName, GetOutBindingFlags());
                mapRules.Value.OutMethodInfo = mapRules.Value.OutPropertyInfo.SetMethod;
            }
            mapRules.Value.CachePrimed = true;
        }
    }
}