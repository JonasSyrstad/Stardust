//
// tabularmapper.cs
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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Stardust.Particles.TableParser;

namespace Stardust.Particles.TabularMapper
{
    class TabularMapper : ITabularMapper
    {
        public IEnumerable<T> MapTo<T>(Document tabularData, MappingDefinition map)
        {
            var convertedData = new List<T>();
            foreach (var row in tabularData.Rows)
            {
                var type = typeof(T);
                var data = Activator.CreateInstance<T>();
                foreach (var fieldMap in map.Fields)
                {
                    var memberInfo = type.GetMember(fieldMap.TargetMemberName, fieldMap.MemberType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).First();
                    type.InvokeMember(fieldMap.TargetMemberName, GetBindingFlags(fieldMap), Type.DefaultBinder, data, new[] { GetValueFromRow<T>(tabularData, row, fieldMap, memberInfo) });
                }
                convertedData.Add(data);
            }
            return convertedData;
        }

        private static object GetValueFromRow<T>(Document tabularData, DocumentRow row, FieldMapping fieldMap, MemberInfo memberInfo)
        {
            var stringData = tabularData.HaveHeader ? row[fieldMap.SourceColumnName] : row[fieldMap.SourceColumnNumber];
            if (memberInfo is PropertyInfo)
                return ValueFromString(memberInfo as PropertyInfo, stringData);
            if (memberInfo is FieldInfo)
                return ValueFromString(memberInfo as FieldInfo, stringData);
            if (memberInfo is MethodInfo)
                return ValueFromString(memberInfo as MethodInfo, stringData);
            return null;
        }

        public static object ValueFromString(MethodInfo protpertyType, string propertyValue)
        {
            var tProp = protpertyType.GetParameters().Single().ParameterType;
            if (tProp.IsGenericType && tProp.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (propertyValue == null)
                {
                    return null;
                }
                tProp = new NullableConverter(tProp).UnderlyingType;
            }
            return Convert.ChangeType(propertyValue, tProp);
        }

        public static object ValueFromString(PropertyInfo protpertyType, string propertyValue)
        {
            var tProp = protpertyType.PropertyType;
            if (tProp.IsGenericType && tProp.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (propertyValue == null)
                {
                    return null;
                }
                tProp = new NullableConverter(protpertyType.PropertyType).UnderlyingType;
            }
            return Convert.ChangeType(propertyValue, tProp);
        }

        public static object ValueFromString(FieldInfo fieldType, string propertyValue)
        {
            var tProp = fieldType.FieldType;
            if (tProp.IsGenericType && tProp.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (propertyValue == null)
                {
                    return null;
                }
                tProp = new NullableConverter(fieldType.FieldType).UnderlyingType;
            }
            return Convert.ChangeType(propertyValue, tProp);
        }

        private static BindingFlags GetBindingFlags(FieldMapping fieldMap)
        {
            if (fieldMap.MemberType == MemberTypes.Property)
                return BindingFlags.Instance | BindingFlags.Public |  BindingFlags.SetProperty;
            if (fieldMap.MemberType == MemberTypes.Field)
                return BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetField;
            return BindingFlags.Instance | BindingFlags.Public |  BindingFlags.InvokeMethod;
        }
    }
}