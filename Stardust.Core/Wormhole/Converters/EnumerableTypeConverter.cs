//
// enumerabletypeconverter.cs
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
using Stardust.Nucleus.ObjectActivator;
using Stardust.Wormhole.MapBuilder;

namespace Stardust.Wormhole.Converters
{
    class EnumerableTypeConverter : TypeConverterBase
    {
        private static Type GenDef;
        protected override object DoConvert(object source, object target = null)
        {
            var genType = Rule.OutPropertyInfo.PropertyType.GetGenericArguments();
            var list = ActivatorFactory.Activator.ActivateWithExternalGenerics(GenericType, genType) as IList;
            var enumerable = source as IEnumerable;
            if (enumerable == null) return list;
            foreach (var item in enumerable)
            {
                EnsurePropertyInfoCache(new KeyValuePair<string, IMapRules>(Rule.InMemberName, Rule));
                if (list != null)
                    list.Add(item.Convert(Rule.Parent.GetRule(item.GetType(), genType[0])));
            }
            return list;
        }

        private static Type GenericType
        {
            get { return GenDef ?? (GenDef = typeof (List<object>).GetGenericTypeDefinition()); }
        }
    }
}