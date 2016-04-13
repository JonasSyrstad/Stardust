//
// configurationmanagerhelpers.cs
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
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;

namespace Stardust.Particles
{
    /// <summary>
    /// Provides cached ConfigurationManager lookup. 
    /// </summary>
    public static class ConfigurationManagerHelper
    {

        public static T ChangeType<T>(this object value, CultureInfo cultureInfo)
        {
            var toType = typeof(T);

            if (value == null) return default(T);

            if (value is string)
            {
                if (toType == typeof(Guid))
                {
                    return ChangeType<T>(new Guid(Convert.ToString(value, cultureInfo)), cultureInfo);
                }
                if ((string)value == string.Empty && toType != typeof(string))
                {
                    return ChangeType<T>(null, cultureInfo);
                }
            }
            else
            {
                if (typeof(T) == typeof(string))
                {
                    return ChangeType<T>(Convert.ToString(value, cultureInfo), cultureInfo);
                }
            }

            if (toType.IsGenericType &&
                toType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                toType = Nullable.GetUnderlyingType(toType); ;
            }

            bool canConvert = toType is IConvertible || (toType.IsValueType && !toType.IsEnum);
            if (canConvert)
            {
                return (T)Convert.ChangeType(value, toType, cultureInfo);
            }
            return (T)value;
        }

        public static T ChangeType<T>(this object value)
        {
            return ChangeType<T>(value, CultureInfo.CurrentCulture);
        }


        private static readonly object Triowing = new object();
        private static readonly ConcurrentDictionary<string, string> AppSettings = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, ConnectionStringSettings> ConnectionSettings = new ConcurrentDictionary<string, ConnectionStringSettings>();
        public static string GetValueOnKey(string key)
        {
            string value;
            if (AppSettings.TryGetValue(key, out value))
                return value;
            lock (Triowing)
            {
                var val = ConfigurationManager.AppSettings[key];
                AppSettings.TryAdd(key, val);
                return val;
            }

        }

        public static string GetValueOnKey(this NameValueCollection appsettings, string key)
        {
            string value;
            if (AppSettings.TryGetValue(key, out value))
                return value;
            lock (Triowing)
            {
                var val = appsettings[key];
                AppSettings.TryAdd(key, val);
                return val; 
            }
        }

        public static T GetValueOnKey<T>(string key,T defaultValue)
        {
            var val = GetValueOnKey(key);
            return val.IsNullOrWhiteSpace() ? defaultValue : val.ChangeType<T>();
        }

        public static T GetValueOnKey<T>(string key)
        {
            return GetValueOnKey(key, default(T));
        }

        public static ConnectionStringSettings GetConnectionStringOnKey(string name)
        {
            ConnectionStringSettings value;
            if (ConnectionSettings.TryGetValue(name, out value)) return value;
            var val = ConfigurationManager.ConnectionStrings[name];
            ConnectionSettings.TryAdd(name, val);
            return val;
        }

        public static ConnectionStringSettings GetConnectionStringOnKey(this ConnectionStringSettingsCollection connectionStrings, string name)
        {
            ConnectionStringSettings value;
            if (ConnectionSettings.TryGetValue(name, out value)) return value;
            var val = connectionStrings[name];
            ConnectionSettings.TryAdd(name, val);
            return val;
        }

        public static void SetValueOnKey(string key, string value)
        {
            SetValueOnKey(key, value, false);
        }

        public static void SetValueOnKey(string key, string value, bool forced)
        {
            if (!forced)
                AppSettings.TryAdd(key, value);
            else
                AppSettings.AddOrUpdate(key, value);
        }
    }
}
