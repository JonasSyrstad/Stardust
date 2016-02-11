//
// IConfigurationReader.cs
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

namespace Stardust.Stellar.ConfigurationReader
{
    /// <summary>
    /// Responsible for reading data from the configuration store.
    /// </summary>
    public interface IConfigurationReader
    {
        /// <summary>
        /// Reads the configSet from the store
        /// </summary>
        /// <param name="setName"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        ConfigurationSet GetConfiguration(string setName,string environment=null);

        /// <summary>
        /// Not implemented in any of the defaults. Not hooked up to the framework, may add support for this at a later stage.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="setName"></param>
        void WriteConfigurationSet(ConfigurationSet configuration, string setName);

        /// <summary>
        /// Gets the original configuration data received from the config service
        /// </summary>
        /// <param name="setName"></param>
        /// <returns></returns>
        T GetRawConfigData<T>(string setName);

        /// <summary>
        /// Gets the original configuration data received from the config service
        /// </summary>
        /// <param name="setName"></param>
        /// <returns></returns>
        object GetRawConfigData(string setName);

        
        IEnumerable<ConfigurationSet> GetAllSets();


        /// <summary>
        /// This will only be used if the Reader implementation supports change notification
        /// </summary>
        /// <param name="onCacheChanged"></param>
        void SetCacheInvalidatedHandler(Action<ConfigurationSet> onCacheChanged);
    }
}