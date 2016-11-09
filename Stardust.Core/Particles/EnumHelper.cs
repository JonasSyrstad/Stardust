//
// EnumHelper.cs
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
using System.Globalization;

namespace Stardust.Particles
{
    public static class EnumHelper
    {
        private static object lockInstance=new object();
        private static Dictionary<Type,Dictionary<int,string>> enumToStringCache=new Dictionary<Type, Dictionary<int, string>>();
        public static string FastToString(this Enum enumVal)
        {
            var enumType = enumVal.GetType();
            Dictionary<int, string> cache;
            if (enumToStringCache.TryGetValue(enumType, out cache)) return cache[enumVal.GetHashCode()];
            var enumValArray = Enum.GetValues(enumType);
            var EnumList = new Dictionary<int, string>();
            foreach (var e in enumValArray)
            {
                EnumList.Add((int)e, e.ToString());
            }
            if (enumToStringCache.TryGetValue(enumType, out cache)) return cache[enumVal.GetHashCode()];
            lock (lockInstance)
            {
                if (enumToStringCache.TryGetValue(enumType, out cache)) return cache[enumVal.GetHashCode()];
                enumToStringCache.Add(enumType, EnumList);
                if (enumToStringCache.TryGetValue(enumType, out cache)) return cache[enumVal.GetHashCode()];
                return EnumList[enumVal.GetHashCode()];
            }
        }
        public static List<T> EnumToList<T>()
        {
            var enumType = GetEnumType<T>();
            var enumValArray = Enum.GetValues(enumType);
            var enumValList = BildEnumList<T>(enumType, enumValArray);
            return enumValList;
        }

        private static Type GetEnumType<T>()
        {
            var enumType = typeof(T);
            return GetEnumType(enumType);
        }

        private static Type GetEnumType(Type enumType)
        {
            if (enumType.BaseType != typeof(Enum)) throw new ArgumentException("T must be of type System.Enum");
            return enumType;
        }

        private static List<T> BildEnumList<T>(Type enumType, Array enumValArray)
        {
            var enumValList = new List<T>(enumValArray.Length);
            foreach (int val in enumValArray)
                enumValList.Add((T) Enum.Parse(enumType, val.ToString(CultureInfo.InvariantCulture)));
            return enumValList;
        }

        public static int[] ToIntArray<T>(this T[] value)
        {
            var result = new int[value.Length];
            for (var i = 0; i < value.Length; i++)
                result[i] = Convert.ToInt32(value[i]);
            return result;
        }

        public static T[] ToEnumArray<T>(this int[] value)
        {
            var result = new T[value.Length];
            for (var i = 0; i < value.Length; i++)
                result[i] = (T)Enum.ToObject(typeof(T), value[i]);
            return result;
        }

        public static T ParseAsEnum<T>(this string value, T defaultValue)
        {
            var val = AsEnum<T>(value);
            if (val.IsNull())return defaultValue;
            return val.Value;
        }

        public static T ParseAsEnum<T>(this string value)
        {
            var val= AsEnum<T>(value);
            if(val.IsNull()) throw new StardustCoreException("Unable to parse enum");
            return val.Value;
        }

        private static EnumContainer<T> AsEnum<T>(string value)
        {
            if (value.IsNullOrWhiteSpace())
                return null;
            if (Enum.IsDefined(typeof (T), value))
                return new EnumContainer<T> { Value = (T)Enum.Parse(typeof(T), value) };
            int num;
            if (int.TryParse(value, out num))
                if (Enum.IsDefined(typeof (T), num))
                    return new EnumContainer<T> {Value = (T) Enum.ToObject(typeof (T), num)};
            return null;
        }

        private class EnumContainer<T>
        {
            internal T Value;
        }
    }
}
