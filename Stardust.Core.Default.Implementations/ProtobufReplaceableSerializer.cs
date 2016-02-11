using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ProtoBuf.Meta;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Serializers;

namespace Stardust.Core.Default.Implementations
{
    /// <summary>
    /// Serializes the call stack to and from the protobuffer format. It uses JSON for <see cref="ConfigurationSet"/> serialization. 
    /// </summary>
    public class ProtobufReplaceableSerializer : IReplaceableSerializer
    {
        private static RuntimeTypeModel protobufModel;
        private static TypeModel compiledModel;

        private readonly JsonSerializerSettings jsonSerializerSettings = JsonReplaceableSerializer.JsonSerializerSettings;

        static ProtobufReplaceableSerializer()
        {
            SetUp();
        }

        public T Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, jsonSerializerSettings);
        }

        private static void SetUp()
        {
            protobufModel = TypeModel.Create();
            AddTypeToModel<CallStackItem>(protobufModel);
            AddTypeToModel<List<CallStackItem>>(protobufModel);
            compiledModel = protobufModel.Compile();
        }
        private static MetaType AddTypeToModel<T>(RuntimeTypeModel typeModel)
        {
            var properties = typeof(T).GetProperties().Select(p => p.Name).OrderBy(name => name);
            return typeModel.Add(typeof(T), true).Add(properties.ToArray());
        }

        /// <summary>
        /// Deserializes call stack data on the client end
        /// </summary>
        /// <param name="callStack">the string representation of the call stack</param>
        /// <returns></returns>
            public CallStackItem Deserialize(string callStack)
            {
                using (var reader = new MemoryStream(Convert.FromBase64String(callStack)))
                {
                    return (CallStackItem)compiledModel.Deserialize(reader, null, typeof(CallStackItem));
                }
            }

        /// <summary>
        /// Serializes the service instance call stack for transport to the client.
        /// </summary>
        /// <param name="value">The call stack item</param>
        /// <returns>the string representation of the call stack</returns>
        public string Serialize(CallStackItem value)
        {
            using (var stream = new MemoryStream())
            {
                compiledModel.Serialize(stream, value);
                return ReadStringFromStream(stream);
            }
        }
        private static string ReadStringFromStream(MemoryStream stream)
        {
            stream.Position = 0;
            var buffer = stream.GetBuffer();
            return Convert.ToBase64String(buffer, 0, (int)stream.Length);
        }

        /// <summary>
        /// Deserializes the json data received from the config service.
        /// </summary>
        /// <param name="payload">the string representation of the configuration data</param>
        /// <returns>The configuration data structure</returns>
        /// <remarks>The Stardust.Starterkit configuration service serializes to json. This is marked as virtual to allow other serialization formats of the <see cref="ConfigurationSet"/></remarks>
        public virtual ConfigurationSet GetConfigurationFromString(string payload)
        {
            using (JsonReader jr = new JsonTextReader(new StringReader(payload)))
            {
                var configSet = (ConfigurationSet)JsonSerializer.CreateDefault(jsonSerializerSettings).Deserialize(jr, typeof(ConfigurationSet));
                return configSet;
            }
        }
    }
}