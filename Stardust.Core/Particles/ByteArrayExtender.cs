//
// ByteArrayExtender.cs
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Stardust.Clusters;

namespace Stardust.Particles
{
    public static class ByteArrayExtender
    {
        private const string FileWriteRetryMessage = "The file (\"{0}\") was locked for edit. Retrying in 10 ms.";
        private const string NumbersOfRetries = "numbers of tries= {0}";
        private const string FileWriteRetrycountConfigKey = "FileWriteRetryCount";

        public static string GetStringFromArray(this byte[] self)
        {
            return GetStringFromArray(self, EncodingType.Utf8);
        }

        public static string GetStringFromArray(this byte[] self, EncodingType encoder)
        {
            var enc = EncodingTypeFactory.CreateEncoder(encoder);
            return enc.GetString(self);
        }

        public static Stream ToStream(this byte[] array)
        {
            return new MemoryStream(array);
        }

        /// <summary>
        /// If the file is locked, the method will try again after 10 ms for eighter 10 times or as many times defined in your config with key "FileWriteRetryCount"
        /// </summary>
        public static void WriteToDisk(this byte[] self, string filePath)
        {
            var retryCnt = GetNumberOfRetries();
            var retryCount = 0;
            var fileSaved = false;
            while (!fileSaved)
            {
                try
                {
                    retryCount++;
                    File.WriteAllBytes(filePath, self);
                    fileSaved = true;
                }
                catch (Exception ex)
                {
                    LogError(filePath, ex, retryCount);
                    Thread.Sleep(10);
                    if (retryCount >= retryCnt)
                        throw;
                }
            }
        }

        private static int GetNumberOfRetries()
        {
            var retryCnt = 10;
            if (ConfigurationManagerHelper.GetValueOnKey(FileWriteRetrycountConfigKey).ContainsCharacters())
                retryCnt = int.Parse(ConfigurationManagerHelper.GetValueOnKey(FileWriteRetrycountConfigKey));
            return retryCnt;
        }

        [ExcludeFromCodeCoverage]
        private static void LogError(string filePath, Exception ex, int retryCount)
        {
            Logging.DebugMessage
                (
                    String.Format(FileWriteRetryMessage, filePath),
                    System.Diagnostics.EventLogEntryType.FailureAudit,
                    string.Format(NumbersOfRetries, retryCount)
                );
            Logging.Exception(ex);
        }

        public static byte[] Reverse(this byte[] self)
        {
            var index = self.Length;
            var outIndex = 0;
            var outArray = new byte[index];
            while (index > 0)
            {
                index--;
                outArray[outIndex] = self[index];
                outIndex++;
            }
            return outArray;
        }
    }
}
