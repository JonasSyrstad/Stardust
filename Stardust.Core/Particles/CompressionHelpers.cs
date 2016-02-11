//
// CompressionHelpers.cs
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
using System.IO.Compression;

namespace Stardust.Particles
{
    public static class CompressionHelpers
    {
        public static void CompressToFile(this byte[] self, string filePath)
        {
            var compressedArray = self.Compress();
            compressedArray.WriteToDisk(filePath);
        }

        public static byte[] Compress(this byte[] self)
        {
            using (var memory = new MemoryStream())
            {
                using (var gzip = new GZipStream(memory, CompressionMode.Compress, true))
                    gzip.Write(self, 0, self.Length);
                return memory.ToArray();
            }
        }

        /// <summary>
        /// Reads and decompresses a gzip file
        /// </summary>
        public static byte[] ReadCompressedFile(string filePath)
        {
            var array = File.ReadAllBytes(filePath);
            return array.Decompress();
        }

        public static byte[] Decompress(this byte[] self)
        {
            using (var baseStream = new MemoryStream(self))
            {
                using (var stream = new GZipStream(baseStream, CompressionMode.Decompress))
                {
                    baseStream.Position = 0;
                    return ReadFromDecompressionStream(stream);
                }
            }
        }

        private static byte[] ReadFromDecompressionStream(Stream stream)
        {
            const int size = 4096;
            var buffer = new byte[size];
            using (var memory = new MemoryStream())
            {
                int count;
                do
                {
                    count = stream.Read(buffer, 0, size);
                    if (count > 0)
                    {
                        memory.Write(buffer, 0, count);
                    }
                } while (count > 0);
                return memory.ToArray();
            }
        }
    }
}