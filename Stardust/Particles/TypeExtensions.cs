//
// typeextensions.cs
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
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Stardust.Particles
{
    public static class TypeExtensions
    {
        internal static PropertyInfo[] GetSetProperties(this Type outGenericType)
        {
            return outGenericType.GetProperties(BindingFlags.Instance | BindingFlags.SetProperty |
                                                BindingFlags.Public | BindingFlags.NonPublic);
        }

        internal static PropertyInfo[] GetReadProperties(this Type innGenericType)
        {
            return innGenericType.GetProperties(BindingFlags.Instance | BindingFlags.GetProperty |
                                                BindingFlags.Public | BindingFlags.NonPublic);
        }

        internal static bool IsValueType(this Type type)
        {

            if (type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(short) ||
                type == typeof(decimal) ||
                type == typeof(string) ||
                type == typeof(double) ||
                type == typeof(DateTime))
            { return true; }
            return false;
        }
        public static bool CanConvert(Type typeOut, Type typeInn)
        {
            if (typeInn == typeOut) return true;
            if (typeOut == typeof(string))
            {
                if (typeInn == typeof(int) ||
                    typeInn == typeof(long) ||
                    typeInn == typeof(short) ||
                    typeInn == typeof(decimal) ||
                    typeInn == typeof(short) ||
                    typeInn == typeof(double) ||
                    typeInn == typeof(DateTime) ||
                    typeInn == typeof(bool))

                    return true;
            }
            return CanConvertTo(typeOut, typeInn) || CanConvertTo(typeInn, typeOut);
        }

        public static bool CanConvertTo(Type typeOut, Type typeInn)
        {
            var typeConverter = TypeDescriptor.GetConverter(typeInn);
            return typeConverter.CanConvertTo(typeOut);
        }

        internal static bool CanConvertFrom(Type typeOut, Type typeInn)
        {
            var typeConverter = TypeDescriptor.GetConverter(typeOut);
            return typeConverter.CanConvertFrom(typeInn);
        }

        public static Func<object, object> BuildGetAccessor(MethodInfo method)
        {
            var obj = Expression.Parameter(typeof(object), "o");
            var expr = Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Call(Expression.Convert(obj, method.DeclaringType), method), typeof(object)), obj);
            return expr.Compile();
        }

        public static Action<object, object> BuildSetAccessor(MethodInfo method)
        {
            var obj = Expression.Parameter(typeof(object), "o");
            var value = Expression.Parameter(typeof(object));

            var expr = Expression.Lambda<Action<object, object>>(Expression.Call(Expression.Convert(obj, method.DeclaringType), method, Expression.Convert(value, method.GetParameters()[0].ParameterType)),obj,value);

            return expr.Compile();
        }

        public static Action<object, object> BuildSetAccessor(MemberInfo member)
        {
            var fieldInfo = member as FieldInfo;
            if (fieldInfo != null)
                return CreateSetAccessor(fieldInfo);
            var propertyInfo = member as PropertyInfo;
            if (propertyInfo != null)
                return BuildSetAccessor(propertyInfo.GetSetMethod());
            return null;
        }

        private static Action<object, object> CreateSetAccessor(FieldInfo field)
        {
            var setMethod = new DynamicMethod(field.Name, typeof(void), new[] { typeof(object), typeof(object) });
            var generator = setMethod.GetILGenerator();
            var local = generator.DeclareLocal(field.DeclaringType);
            generator.Emit(OpCodes.Ldarg_0);
            if (field.DeclaringType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, field.DeclaringType);
                generator.Emit(OpCodes.Stloc_0, local);
                generator.Emit(OpCodes.Ldloca_S, local);
            }
            else
            {
                generator.Emit(OpCodes.Castclass, field.DeclaringType);
                generator.Emit(OpCodes.Stloc_0, local);
                generator.Emit(OpCodes.Ldloc_0, local);
            }
            generator.Emit(OpCodes.Ldarg_1);
            if (field.FieldType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, field.FieldType);
            }
            else
            {
                generator.Emit(OpCodes.Castclass, field.FieldType);
            }
            generator.Emit(OpCodes.Stfld, field);
            generator.Emit(OpCodes.Ret);
            return (Action<object, object>)setMethod.CreateDelegate(typeof(Action<object, object>));
        }

    }
}