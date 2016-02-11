//
// FileConfigurationReader.cs
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
using System.IO;
using System.Linq;
using Stardust.Particles;
using Stardust.Particles.Xml;

namespace Stardust.Stellar.ConfigurationReader
{
    public class FileConfigurationReader : IConfigurationReader
    {
        private ConfigurationSets ConfigurationData;

        public FileConfigurationReader()
        {
            if (ConfigurationData.IsNull())
            {
                if (File.Exists(LocalConfigStore.GetConfigFilePath()))
                    ConfigurationData = Deserializer<ConfigurationSets>.GetInstanceFromFile(LocalConfigStore.GetConfigFilePath());
                else
                {
                    ConfigurationData = new ConfigurationSets();
                    Write();
                }
            }
        }

        private void Write()
        {
            ConfigurationData.SerializeToFile(LocalConfigStore.GetConfigFilePath(), true);
        }

        public ConfigurationSet GetConfiguration(string setName, string environment = null)
        {

            return ConfigurationData.GetConfigSet(setName);
        }

        

        public void WriteConfigurationSet(ConfigurationSet configuration, string setName)
        {
            configuration.LastUpdated = DateTime.Now;
            ConfigurationData.AddOrUpdate(setName, configuration);
            Write();
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
            return (from cs in ConfigurationData.ConfigSets orderby cs.Id ascending select cs).ToList();
        }

        

        public void SetCacheInvalidatedHandler(Action<ConfigurationSet> onCacheChanged)
        {
            
        }
    }
}
