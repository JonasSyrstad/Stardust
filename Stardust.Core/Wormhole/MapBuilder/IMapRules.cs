//
// imaprules.cs
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
using Stardust.Wormhole.Converters;

namespace Stardust.Wormhole.MapBuilder
{
    public delegate object GetDelegate(object arg);

    public interface IMapRules
    {
        string OutMemberName { get; set; }
        Dictionary<string, Func<object, object>> DottedExpressions { get; }

        bool Convertible { get; set; }

        PropertyInfo InPropertyInfo { get; set; }

        PropertyInfo OutPropertyInfo { get; set; }

        BasicTypes BasicType { get; set; }

        IMapContainer Parent { get; set; }

        string InMemberName { get; set; }

        Func<object, object> GetExpression { get; }

        Action<object, object> SetExpression { get; }

        ITypeConverter Converter { get; set; }

        Type[] OutGenericDefinition { get; set; }

        bool CachePrimed { get; set; }

        bool BaseCachePrimed { get; set; }

        MethodInfo InMethodInfo { get; set; }

        MethodInfo OutMethodInfo { get; set; }


        bool DottedSource { get; set; }

        Func<object, object> GetDotExpressionPart(string subItem,Type subType);
    }
}