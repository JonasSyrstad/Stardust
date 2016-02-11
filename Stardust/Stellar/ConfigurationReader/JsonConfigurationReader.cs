//
// jsonconfigurationreader.cs
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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Threading;
using Newtonsoft.Json;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Stellar.ConfigurationReader
{
    public class JsonConfigurationReader : IConfigurationReader
    {
        private static int? ExpireTime;

        public ConfigurationSet GetConfiguration(string setName, string environment = null)
        {
            var faultedMemCache = validateMemCache();
            if (faultedMemCache)
                Logging.DebugMessage("ConfigSet MemoryCache error.....");
            ConfigurationSet item = null;
            if (!faultedMemCache)
                item = (ConfigurationSet)MemoryCache.Default.Get(setName);
            if (item.IsInstance()) return item;
            item = GetConfigurationSet(setName, environment);
            Logging.DebugMessage("[ConfigSet cache is not primed]");
            if (!faultedMemCache)
                MemoryCache.Default.Set(setName, item, new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 0, GetCacheExpirationTime()) });
            return item;
        }

        private static bool validateMemCache()
        {
            var faultedMemCache = false;
            var threadDelegate = new ThreadStart(() => GetCacheCount());
            var thread_ = new Thread(threadDelegate);
            thread_.Start();
            var timer = Stopwatch.StartNew();
            while (thread_.IsAlive)
            {
                if (timer.ElapsedMilliseconds > 20)
                {
                    try
                    {
                        thread_.Abort();
                        thread_.Join();
                        faultedMemCache = true;
                    }
                    catch
                    {
                        faultedMemCache = true;
                    }
                }
            }
            return faultedMemCache;
        }

        private static int GetCacheCount()
        {
            try
            {
                return MemoryCache.Default.Count();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private int GetCacheExpirationTime()
        {
            if (ExpireTime != null) return ExpireTime.Value;
            var expTime = ConfigurationManagerHelper.GetValueOnKey("stardust.ConfigCacheExpity");
            int val;
            if (!int.TryParse(expTime, out val))
                val = 1000;
            ExpireTime = val;
            return ExpireTime.Value;
        }

        private ConfigurationSet GetConfigurationSet(string setName, string environment)
        {
            using (var wc = new WebClient())
            {
                //Logging.DebugMessage("Web client obtained");
                SetCredentials(wc);
                var url = string.Format("{0}/api/ConfigReader/{1}?env={2}", LocalConfigStore.GetConfigServiceUrl(), setName, environment);
                //Logging.DebugMessage(string.Format("Reading settings from '{0}'", url));
                var payload = wc.DownloadString(url);
                //Logging.DebugMessage("Config set loaded");
                return GetConfigurationFromString(payload);
            }
        }

        private static ConfigurationSet GetConfigurationFromString(string payload)
        {
            return JsonConvert.DeserializeObject<ConfigurationSet>(payload);

        }

        private static void SetCredentials(WebClient wc)
        {
            var userName = LocalConfigStore.GetConfigServiceUser();
            var password = LocalConfigStore.GetConfigServicepassword();
            if (userName.ContainsCharacters() && password.ContainsCharacters())
            {
                wc.Credentials = new NetworkCredential(userName, password, "EAZYPORTCONFIG");
            }
        }

        public void WriteConfigurationSet(ConfigurationSet configuration, string setName)
        {
            throw new NotImplementedException();
        }

        public T GetRawConfigData<T>(string setName)
        {
            throw new NotImplementedException();
        }

        public object GetRawConfigData(string setName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ConfigurationSet> GetAllSets()
        {
            throw new NotImplementedException();
        }

        

        public void SetCacheInvalidatedHandler(Action<ConfigurationSet> onCacheChanged)
        {
            
        }

        public static bool NoCache
        {
            get { return ConfigurationManagerHelper.GetValueOnKey("Stardust.NoCache") == "true"; }
        }
    }
}