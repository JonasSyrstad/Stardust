//
// deserializer.cs
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

using System.IO;
using System.Text;
using Stardust.Clusters;

namespace Stardust.Particles.Xml
{
    public sealed class Deserializer<T>
    {
        public static T Deserialize(Stream storageStream, string defaultNamespace = SerializationHelper.DefaultNamespace,bool includeTypeInNamespace=false)
        {
            var type = typeof(T);
            var serializer = SerializerExtensions.CreateXmlSerializer(type,defaultNamespace,includeTypeInNamespace);
            return (T)serializer.Deserialize(storageStream);
        }

        public static T GetInstanceFromFile(string path, string defaultNamespace = SerializationHelper.DefaultNamespace)
        {
            using (var fileStream = File.OpenRead(path))
            {
                return Deserialize(fileStream, defaultNamespace);
            }
        }

        public static bool TryGetInstanceFromFile(string path, ref T instance, string defaultNamespace = SerializationHelper.DefaultNamespace)
        {
            try
            {
                var data = EncodingFactory.ReadFileText(path);
                instance = Deserialize(data, defaultNamespace);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public static T Deserialize(string data, string defaultNamespace = SerializationHelper.DefaultNamespace)
        {
            using (Stream memoryStream = new MemoryStream())
            {
                FillMemoryStream(data, memoryStream);
                return Deserialize(memoryStream, defaultNamespace);
            }
        }

        private static void FillMemoryStream(string data, Stream memoryStream)
        {
            var storage = new StreamWriter(memoryStream, new UTF8Encoding());
            storage.Write(data);
            storage.Flush();
            memoryStream.Position = 0;
        }
    }
}