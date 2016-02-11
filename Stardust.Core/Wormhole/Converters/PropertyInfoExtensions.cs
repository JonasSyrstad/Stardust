//
// propertyinfoextensions.cs
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

namespace Stardust.Wormhole.Converters
{
    internal static class PropertyInfoExtensions
    {

        //public static object Accessvalue(this PropertyInfo propertyInfo, object @from, KeyValuePair<string, IMapRules> mapRules)
        //{
        //    if (mapRules.Value.GetExpression == null)
        //        mapRules.Value.GetExpression = propertyInfo.GetValueGetter();
        //    return mapRules.Value.GetExpression.DynamicInvoke(new[] { from });
        //}
        //public static Delegate GetValueGetter(this PropertyInfo propertyInfo)
        //{

        //    var instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
        //    var property = Expression.Property(instance, propertyInfo);
        //    var convert = Expression.TypeAs(property, typeof(object));
        //    return Expression.Lambda(convert, instance).Compile();

        //}
        //public static Delegate GetValueSetter(this PropertyInfo propertyInfo)
        //{
        //    var instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
        //    var argument = Expression.Parameter(typeof(object), "a");
        //    var setterCall = Expression.Call(instance, propertyInfo.GetSetMethod(), Expression.Convert(argument, propertyInfo.PropertyType));
        //    return Expression.Lambda(setterCall, instance, argument).Compile();
        //}


        //internal static void SaveValue(this PropertyInfo propertyInfo, object target, object value, KeyValuePair<string, IMapRules> mapRules)
        //{
        //    if (mapRules.Value.SetExpression == null)
        //        mapRules.Value.SetExpression = propertyInfo.GetValueSetter();
        //    mapRules.Value.SetExpression.DynamicInvoke(new[] { target, value });
        //}

    }
}