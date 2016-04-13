//
// defaultcalstackserializer.cs
// This file is part of Stardust.Core.Default.Implementations
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

using System.IO;
using Newtonsoft.Json;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Serializers;

namespace Stardust.Core.Default.Implementations
{
    /// <summary>
    /// Serializes ConfigurationSet and CallStackItem using JSON
    /// </summary>
    public class JsonReplaceableSerializer : IReplaceableSerializer
    {
        internal static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
                                                                    {
                                                                        MissingMemberHandling = MissingMemberHandling.Ignore,
                                                                        NullValueHandling = NullValueHandling.Ignore,
                                                                        Formatting = Formatting.None

                                                                    };

        /// <summary>
        /// Deserializes call stack data on the client end
        /// </summary>
        /// <param name="callStack">the string representation of the call stack</param>
        /// <returns></returns>
        public CallStackItem Deserialize(string callStack)
        {
            return (CallStackItem)JsonConvert.DeserializeObject(callStack, typeof(CallStackItem),JsonSerializerSettings);
        }

        public T Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value,JsonSerializerSettings);
        }

        /// <summary>
        /// Serializes the service instance call stack for transport to the client.
        /// </summary>
        /// <param name="value">The call stack item</param>
        /// <returns>the string representation of the call stack</returns>
        public string Serialize(CallStackItem value)
        {
            return JsonConvert.SerializeObject(value,JsonSerializerSettings);
        }

        public string SerializeObject<T>(T item)
        {
            return JsonConvert.SerializeObject(item);
        }

        /// <summary>
        /// Deserializes the json data received from the config service.
        /// </summary>
        /// <param name="payload">the string representation of the configuration data</param>
        /// <returns>The configuration data structure</returns>
        /// <remarks>The Stardust.Starterkit configuration service serializes to json.</remarks>
        public ConfigurationSet GetConfigurationFromString(string payload)
        {
            using (JsonReader jr = new JsonTextReader(new StringReader(payload)))
                return (ConfigurationSet)JsonSerializer.CreateDefault(JsonSerializerSettings).Deserialize(jr, typeof(ConfigurationSet));
        }
    }
}
