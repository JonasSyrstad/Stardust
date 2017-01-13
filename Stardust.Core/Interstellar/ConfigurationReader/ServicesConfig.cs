//
// ServicesConfig.cs
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
    public class ServiceConfig
    {
        [Key]
        public long Id { get; set; }

        public string ServiceName { get; set; }

        public List<ConfigParameter> ConnectionStrings { get; set; }

        private readonly ConcurrentDictionary<string, ConfigParameter> ParameterCache = new ConcurrentDictionary<string, ConfigParameter>(); 

        public List<ConfigParameter> Parameters { get; set; }

        public string GetConfigParameter(string name)
        {
            var val = GetValue(name);
            if (val.IsNull()) return "";
            return val.Value;
        }

        private ConfigParameter GetValue(string name)
        {
            ConfigParameter param;
            if (ParameterCache.TryGetValue(string.Format("{0}_{1}", ServiceName, name).ToLower(), out param)) return param;
            param = (from p in Parameters where Equals(p.Name, name) select p).FirstOrDefault();
            ParameterCache.TryAdd(string.Format("{0}_{1}", ServiceName, name).ToLower(), param);
            return param;
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
            catch (Exception)
            {
                if (value.BinaryValue.IsNull()) return null;
                return value.BinaryValue.GetStringFromArray(EncodingType.Unicode).Decrypt(KeyHelper.SharedSecret);
            }

        }

        public string Password
        {
            get
            {
                if (Parameters.IsNull())
                    Parameters = new List<ConfigParameter>();
                try
                {
                    return GetSecureConfigParameter("ServiceAccountPassword");
                }
                catch
                {
                    return string.Empty;
                }
            }
            set
            {
                if (value.IsNullOrWhiteSpace())
                {
                    PasswordIsUnchanged = true;
                    return;
                }
                if (Parameters.IsNull()) Parameters = new List<ConfigParameter>();
                var password = (from p in Parameters where string.Equals(p.Name, "ServiceAccountPassword", StringComparison.OrdinalIgnoreCase) select p).FirstOrDefault();
                var encrypted = value.Encrypt(KeyHelper.SharedSecret);
                if (password.IsNull())
                {
                    password = new ConfigParameter { Name = "ServiceAccountPassword" };
                    Parameters.Add(password);
                }
                password.Value = encrypted;
                password.BinaryValue = encrypted.GetByteArray();
                password.Value = value.Encrypt(KeyHelper.SharedSecret);
                password.BinaryValue = password.Value.GetByteArray();
                if (password.Value.Decrypt(KeyHelper.SharedSecret) != value) throw new StardustCoreException("Encryption validation failed!");
            }
        }
        public string Username
        {
            get
            {
                if (Parameters.IsNull())
                    Parameters = new List<ConfigParameter>();
                return GetConfigParameter("ServiceAccountName");
            }
            set
            {
                if (Parameters.IsNull())
                    Parameters = new List<ConfigParameter>();
                var accountName = (from p in Parameters where string.Equals(p.Name, "ServiceAccountName", StringComparison.OrdinalIgnoreCase) select p).FirstOrDefault();
                if (accountName.IsNull())
                {
                    accountName = new ConfigParameter
                                        {
                                            Name = "ServiceAccountName",
                                            Value = value
                                        };
                    Parameters.Add(accountName);
                }
                else
                {
                    accountName.Value = value;
                }
            }
        }

        public bool PasswordIsUnchanged { get; set; }

        public IdentitySettings IdentitySettings { get; set; }
    }
}
