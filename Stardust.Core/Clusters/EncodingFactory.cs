//
// EncodingFactory.cs
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Particles.Collection.Arrays;

namespace Stardust.Clusters
{
    /// <summary>
    /// Contains methods to find the correct encoding for files.
    /// </summary>
    public static class EncodingFactory
    {
        private const string NoFilenameProvided = "No filename provided";

        private const string NoStreamProvided = "No stream provided";

        private const string NoDataProvided = "No data provided";

        private const string Ansi = "Ansi";

        private static readonly List<IEncodingChecker> Checkers = new List<IEncodingChecker>();

        public static string RemoveFileMarkers(this string self)
        {
            if (self.IsNullOrEmpty()) return string.Empty;
            if (self.DoesNotContainBoom()) return self;
            return self.Remove(0, 1);
        }

        public static string ReadFileText(string fileName)
        {
            if (fileName.IsNullOrEmpty()) throw new StardustCoreException(NoFilenameProvided);
            return Check(fileName)
                .ReadText(fileName)
                .RemoveFileMarkers();
        }

        public static string ReadFileText(Stream stream)
        {
            if (stream.IsNull()) throw new StardustCoreException(NoStreamProvided);
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
            stream.Position = 0;
            return Check(buffer, buffer.Length)
                .ReadText(stream)
                .RemoveFileMarkers();
        }

        public static string ReadFileText(byte[] buffer)
        {
            if (ArrayExtensions.IsEmpty(buffer)) throw new StardustCoreException(NoDataProvided);
            return Check(buffer, buffer.Length)
                .GetString(buffer)
                .RemoveFileMarkers();
        }

        public static Encoding Check(string fileName)
        {
            var checker = GetChecker(fileName);
            return GetEncoding(checker);
        }

        public static Encoding Check(Stream stream)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
            stream.Close();
            var checker = GetChecker(buffer, buffer.Length);
            return GetEncoding(checker);
        }

        public static Encoding Check(byte[] buffer, int length)
        {
            var checker = GetChecker(buffer, length);
            return GetEncoding(checker);
        }

        [ExcludeFromCodeCoverage]
        public static string GetEncodingName(string fileName)
        {
            var checker = GetChecker(fileName);
            return GetEncodingName(checker);
        }

        [ExcludeFromCodeCoverage]
        public static string GetEncodingName(Stream stream)
        {
            var checker = GetChecker(stream);
            return GetEncodingName(checker);
        }

        [ExcludeFromCodeCoverage]
        public static string GetEncodingName(byte[] buffer, int length)
        {
            var checker = GetChecker(buffer, length);
            return GetEncodingName(checker);
        }

        private static bool DoesNotContainBoom(this string self)
        {
            return self[0] != 65279;
        }

        private static string ReadText(this Encoding self, string filePath)
        {
            return self.GetString(File.ReadAllBytes(filePath));
        }

        private static string ReadText(this Encoding self, Stream stream)
        {
            byte[] buffer;
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                buffer= ms.ToArray();
            }
            return self.GetString(buffer);
        }

        private static IEncodingChecker GetChecker(string fileName)
        {
            return GetChecker(c => c.Check(fileName));
        }

        private static IEncodingChecker GetChecker(Func<IEncodingChecker, bool> criteria)
        {
            var checker = from c in GetCheckers()
                          where criteria(c)
                          select c;
            return checker.FirstOrDefault();
        }

        private static IEnumerable<IEncodingChecker> GetCheckers()
        {
            if (ArrayExtensions.IsEmpty(Checkers))
                lock (Checkers)
                    CreateAndAddCheckers();
            return Checkers;
        }

        private static void CreateAndAddCheckers()
        {
            if (ArrayExtensions.ContainsElements(Checkers)) return;
            Checkers.AddRange(Resolver.ActivateAll<IEncodingChecker>());
            //Checkers.AddRange(from c in Resolver.ResolveAllOf<IEncodingChecker>() select c.Activate<IEncodingChecker>());
        }

        [ExcludeFromCodeCoverage]
        private static IEncodingChecker GetChecker(Stream stream)
        {
            try
            {
                return GetChecker(c => c.Check(stream));
            }
            finally
            {
                stream.Close();
            }
        }

        private static IEncodingChecker GetChecker(byte[] buffer, int length)
        {
            return GetChecker(c => c.Check(buffer, length));
        }

        private static Encoding GetEncoding(IEncodingChecker checker)
        {
            return checker.IsInstance() ? checker.GetEncoding() : Encoding.Default;
        }

        [ExcludeFromCodeCoverage]
        private static string GetEncodingName(IEncodingChecker checker)
        {
            return checker.IsInstance() ? checker.GetEncodingName() : Ansi;
        }
    }
}