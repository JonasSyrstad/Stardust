﻿using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stardust.Core.Azure
{
    public static class JsonSerializerHelper
    {
        /// <summary>
        /// Serialize an object using the DataContractJsonSerializer.
        /// </summary>
        /// <param name="item">The object that must be serialized</param>
        /// <param name="formatting">Indicates out the output is formatted</param>
        /// <returns>A Json representation of the object passed as an argument.</returns>
        public static string Serialize(object item, Formatting formatting = default(Formatting))
        {
            if (item == null)
            {
                throw new ArgumentException("The item argument cannot be null.");
            }

            var json = JsonConvert.SerializeObject(item, formatting);
            return json;
        }

        /// <summary>
        /// Deserialize a JSON string into an object using the JavaScriptSerializer.
        /// </summary>
        /// <param name="item">The string that must be deserialized.</param>
        /// <returns>The object deserialized.</returns>
        public static T Deserialize<T>(string item)
        {
            if (item == null)
            {
                throw new ArgumentException("The item argument cannot be null.");
            }
            return JsonConvert.DeserializeObject<T>(item);
        }

        /// <summary>
        /// Deserialize an Json string into an object using the JavaScriptSerializer.
        /// </summary>
        /// <param name="stream">The stream that must be deserialized.</param>
        /// <returns>The object deserialized.</returns>
        public static T Deserialize<T>(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentException("The stream argument cannot be null.");
            }

            string item;
            using (var reader = new StreamReader(stream))
            {
                item = reader.ReadToEnd();
            }
            return Deserialize<T>(item);
        }

        /// <summary>
        /// Checks if the string is in JSON format.
        /// </summary>
        /// <param name="item">The string that must be deserialized.</param>
        /// <returns>Returns true if the object is in JSON format, false otherwise.</returns>
        public static bool IsJson(string item)
        {
            if (item == null)
            {
                throw new ArgumentException("The item argument cannot be null.");
            }
            try
            {
                var obj = JToken.Parse(item);
                return obj != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}