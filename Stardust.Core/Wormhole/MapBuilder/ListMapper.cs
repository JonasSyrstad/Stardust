//
// listmapper.cs
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

namespace Stardust.Wormhole.MapBuilder
{
    internal class ListMapper<TInn, TOut>
    {
        private readonly IMapContainer Parent;

        public ListMapper(IMapContainer parent)
        {
            Parent = parent;
        }

        internal void MapIEnumerable(IAutomapRules map, PropertyInfo innPropertyInfo, PropertyInfo outPropertyInfo)
        {
            map.Fields.Add(new KeyValuePair<string, IMapRules>(innPropertyInfo.Name,
                new MapRules
                {
                    InMemberName = innPropertyInfo.Name,
                    OutMemberName = outPropertyInfo.Name,
                    BasicType = BasicTypes.List,
                    Parent = Parent
                }));
            var innGenericType = innPropertyInfo.PropertyType.GetGenericArguments()[0];
            var outGenericType = outPropertyInfo.PropertyType.GetGenericArguments()[0];
            if (!map.Parent.ContainsRule(new KeyValuePair<Type, Type>(innGenericType, outGenericType)))
            {
                map.Parent.AddMap(innGenericType, outGenericType);
            }
        }
    }
}