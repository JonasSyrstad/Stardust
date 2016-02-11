//
// filesigning.cs
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

using System.Security.Cryptography;

namespace Stardust.Core.Security
{
    public class FileSigning
    {
        public static byte[] CreateNewPrivateCertificate()
        {
            var provider = new RSACryptoServiceProvider();
            return provider.ExportCspBlob(true);
        }

        public static byte[] GetPublicCertificate(byte[] privateCertificate)
        {
            var provider = LoadProvider(privateCertificate);
            return provider.ExportCspBlob(false);
        }

        private static RSACryptoServiceProvider LoadProvider(byte[] privateCertificate)
        {
            var provider = new RSACryptoServiceProvider();
            provider.ImportCspBlob(privateCertificate);
            return provider;
        }

        public static byte[] SignDocument(byte[] document, byte[] certificate)
        {
            var provider = LoadProvider(certificate);
            return provider.SignData(document, new SHA256CryptoServiceProvider());
        }

        public static bool Validate(byte[] document, byte[] publicCertificate, byte[] signature)
        {
            var provider = LoadProvider(publicCertificate);
            return provider.VerifyData(document, new SHA256CryptoServiceProvider(), signature);
        }

        public static byte[] Encrypt(byte[] document, byte[] privateCertificate)
        {
            var provider = LoadProvider(privateCertificate);
            return provider.Encrypt(document, false);
        }

        public static byte[] Decrypt(byte[] document, byte[] privateCertificate)
        {
            var provider = LoadProvider(privateCertificate);
            return provider.Decrypt(document, false);
        }
    }
}