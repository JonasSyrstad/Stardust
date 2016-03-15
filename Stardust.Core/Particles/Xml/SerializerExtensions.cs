//
// SerializerExtensions.cs
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
using System.IO;
using System.Xml.Serialization;
using Stardust.Core;

namespace Stardust.Particles.Xml
{
    public static class SerializerExtensions
    {
        /// <summary>
        /// Note that only classes marked with the Serializable attribute will be included in the parameterless version
        /// </summary>
        public static void SerializeToFile(this object self, string filePath, bool forceSerializationAnyway = false)
        {
            self.SerializeToFile(filePath, SerializationHelper.DefaultNamespace, forceSerializationAnyway);
        }

        /// <summary>
        /// Note that only classes marked with the Serializable attribute will be included in the parameterless version
        /// </summary>
        public static void SerializeToFile(this object self, string filePath, string defaultNamespace = SerializationHelper.DefaultNamespace, bool forceSerializationAnyway = false, bool includeTypeInNamespace = false)
        {
            var serializedVersion = self.Serialize(defaultNamespace, forceSerializationAnyway,includeTypeInNamespace);
            serializedVersion.WriteToFile(filePath);
        }

        /// <summary>
        /// Note that only classes marked with the Serializable attribute will be included in the parameterless version
        /// </summary>
        public static string Serialize(this object self, string defaultNamespace = SerializationHelper.DefaultNamespace, bool forceSerializationAnyway = false, bool includeTypeInNamespace = false)
        {
            if (self.ShouldTryToSerialize(forceSerializationAnyway))
            {
                return SerializeObject(self, defaultNamespace,includeTypeInNamespace);
            }
            throw new InvalidOperationException("Not a serializable object");
        }

        private static bool ShouldTryToSerialize(this object self, bool forceSerializationAnyway)
        {
            return self.IsSerializable() || forceSerializationAnyway;
        }

        /// <summary>
        /// Note that only classes marked with the Serializable attribute will return true. The object may still be serializable.
        /// </summary>
        public static bool IsSerializable(this object self)
        {
            var type = self.GetType();
            return type.IsSerializable;
        }

        private static string SerializeObject(object self, string defaultNamespace, bool includeTypeInNamespace = false)
        {
            var serializer = CreateXmlSerializer(self.GetType(), defaultNamespace,includeTypeInNamespace);
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, self);
                return ReadStringFromStream(stream);
            }
        }

        private static string ReadStringFromStream(MemoryStream stream)
        {
            stream.Position = 0;
            using (var sr = new StreamReader(stream))
                return sr.ReadToEnd();
        }

        internal static XmlSerializer CreateXmlSerializer(Type self, string defaultNamespace, bool includeTypeInNamespace = false)
        {
            var usedNamespace = includeTypeInNamespace?SerializationHelper.BuildNamespace(defaultNamespace, self):defaultNamespace;
            XmlSerializer serializer;
            if(SerializerCache.TryGetValue(GetSerializerKey(self, usedNamespace),out serializer)) return serializer;
            serializer = new XmlSerializer(self, usedNamespace);
            SerializerCache.TryAdd(GetSerializerKey(self, usedNamespace), serializer);
            return serializer;
        }

        internal static string GetSerializerKey(Type self, string usedNamespace)
        {
            return string.Format("{0}_{1}", self.FullName, usedNamespace);
        }

        private static ConcurrentDictionary<string,XmlSerializer> SerializerCache=new ConcurrentDictionary<string, XmlSerializer>(); 
    }
}
