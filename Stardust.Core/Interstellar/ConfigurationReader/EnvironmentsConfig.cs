//
// EnvironmentsConfig.cs
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Stardust.Particles;

namespace Stardust.Interstellar.ConfigurationReader
{
    public class EnvironmentConfig
    {
        private readonly ConcurrentDictionary<string , ConfigParameter> parameterCache=new ConcurrentDictionary<string, ConfigParameter>();

        [Key]
        public long Id { get; set; }

        public string EnvironmentName { get; set; }

        public virtual List<ConfigParameter> Parameters { get; set; }

        public virtual ConfigurationSet Set { get; set; }

        public string GetConfigParameter(string name)
        {
            var val = GetValue(name);
            if (val.IsNull()) return "";
            return val.Value;
        }

        private ConfigParameter GetValue(string name)
        {
            ConfigParameter value;
            if (parameterCache.TryGetValue(name.ToLower(), out value)) return value;
            value= (from p in Parameters where string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase) select p).FirstOrDefault();
            parameterCache.TryAdd(name.ToLower(), value);
            return value;
        }

        public string GetSecureConfigParameter(string name)
        {
            var value = GetValue(name);
            if (value.IsNull()) return null;
            try
            {
                if (value.Value.ContainsCharacters())
                    return value.Value.Decrypt(KeyHelper.SharedSecret);
                if (value.BinaryValue.IsNull()) return null;
                return value.BinaryValue.GetStringFromArray(EncodingType.Utf8).Decrypt(KeyHelper.SharedSecret);
            }
            catch (System.Exception)
            {
                if (value.BinaryValue.IsNull()) return null;
                return value.BinaryValue.GetStringFromArray(EncodingType.Unicode).Decrypt(KeyHelper.SharedSecret);
            }
        }

        public CacheSettings Cache { get; set; }

        public string ReaderKey { get; set; }
    }
}

