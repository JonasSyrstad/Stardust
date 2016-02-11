//
// dictionarymapper.cs
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

namespace Stardust.Wormhole.MapBuilder
{
    internal class DictionaryMapper<TInn, TOut>
    {
        private readonly IMapContainer Parent;

        public DictionaryMapper(IMapContainer parent)
        {
            Parent = parent;
        }

        internal void MapDictionary(IAutomapRules map, PropertyInfo innPropertyInfo, PropertyInfo outPropertyInfo)
        {
            var innGenericType = innPropertyInfo.PropertyType.GetGenericArguments()[0];
            var outGenericType = outPropertyInfo.PropertyType.GetGenericArguments()[0];
            var innGenericType2 = innPropertyInfo.PropertyType.GetGenericArguments()[1];
            var outGenericType2 = outPropertyInfo.PropertyType.GetGenericArguments()[1];
            if (TypeExtensions.CanConvert(outGenericType, innGenericType) &&
                TypeExtensions.CanConvert(outGenericType2, innGenericType2))
                map.Fields.Add(new KeyValuePair<string, IMapRules>(innPropertyInfo.Name,
                    new MapRules
                    {
                        InMemberName = innPropertyInfo.Name,
                        OutMemberName = outPropertyInfo.Name,
                        Convertible = true,
                        BasicType = BasicTypes.Dictionary,
                        Parent = Parent
                    }));
            else if (TypeExtensions.CanConvert(outGenericType, innGenericType))
            {
                map.Fields.Add(new KeyValuePair<string, IMapRules>(innPropertyInfo.Name,
                    new MapRules
                    {
                        InMemberName = innPropertyInfo.Name,
                        OutMemberName = outPropertyInfo.Name,
                        Convertible = false,
                        BasicType = BasicTypes.Dictionary,
                        Parent = Parent
                    }));
                if (!map.Parent.ContainsRule(new KeyValuePair<Type, Type>(innGenericType2, outGenericType2)))
                {
                    map.Parent.AddMap(innGenericType2, outGenericType2);
                }
            }
        }
    }
}