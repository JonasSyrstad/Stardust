//
// iautomaprules.cs
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
using System.Linq.Expressions;

namespace Stardust.Wormhole.MapBuilder
{
    public interface IAutomapRules : IMapRules
    {
        Type InType { get; set; }

        Type OutType { get; set; }

        List<KeyValuePair<string, IMapRules>> Fields { get; }
        

        IAutomapRules<TI, TO> GetRule<TI, TO>();
    }

    public interface IAutomapRules<TInn, TOut> : IAutomapRules
    {
        IAutomapRules<TInn, TOut> Add<TInnProperty, TOutProperty>(Expression<Func<TInn, TInnProperty>> inPropertyLambda, Expression<Func<TOut, TOutProperty>> outPropertyLambda, BasicTypes convertionType = BasicTypes.Convertable);
        IAutomapRules<TInn, TOut> Add(string sourceField, string targetField, BasicTypes convertionType);
        IAutomapRules<TInn, TOut> RemoveMapping(string sourceFieldToRemove);
    }
}