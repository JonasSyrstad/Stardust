//
// CryptoService.cs
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
using System.Security.Cryptography;
using Stardust.Core.Security;

namespace Stardust.Particles
{
    public static class CryptoService
    {
        private delegate ICryptoTransform TransformCreator(TripleDESCryptoServiceProvider tdesAlgorithm);

        static CryptoService()
        {
            Salt = ConfigurationManagerHelper.GetValueOnKey("stardust.CSalt");
            if (Salt.IsNullOrWhiteSpace()) Salt = "tinkerBell";
        }

        private static string Salt { get; set; }

        public static string Encrypt(this string message, EncryptionKeyContainer sharedSecret)
        {
            return Convert.ToBase64String(message.GetByteArray(EncodingType.Unicode).Encrypt(sharedSecret));
        }

        public static byte[] Encrypt(this byte[] file, EncryptionKeyContainer sharedSecret)
        {
            return file.RunCryptoService(sharedSecret, CreateEncryptTransform);
        }

        private static ICryptoTransform CreateEncryptTransform(TripleDESCryptoServiceProvider tdesAlgorithm)
        {
            return tdesAlgorithm.CreateEncryptor();
        }

        private static byte[] RunCryptoService(this byte[] file, EncryptionKeyContainer sharedSecret, TransformCreator createCryptoTransform)
        {
            using (var hashProvider = new MD5CryptoServiceProvider())
            {
                using (var tdesAlgorithm = hashProvider.CreateKey(sharedSecret).CreateAlgorithm())
                {
                    return TransformFinalBlock(file, createCryptoTransform, tdesAlgorithm);
                }
            }
        }

        private static byte[] CreateKey(this HashAlgorithm hashProvider, EncryptionKeyContainer sharedSecret)
        {
            return hashProvider.ComputeHash(GetFileSecret(sharedSecret).GetByteArray());
        }

        private static TripleDESCryptoServiceProvider CreateAlgorithm(this byte[] tdesKey)
        {
            return new TripleDESCryptoServiceProvider
            {
                Key = tdesKey,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
        }

        private static byte[] TransformFinalBlock(byte[] file, TransformCreator createCryptoTransform, TripleDESCryptoServiceProvider tdesAlgorithm)
        {
            try
            {
                using (var encryptor = createCryptoTransform(tdesAlgorithm))
                    return encryptor.TransformFinalBlock(file, 0, file.Length);
            }
            catch (Exception ex)
            {
                throw new StardustCoreException("Crypto operation failed", ex);
            }
        }

        private static string GetFileSecret(EncryptionKeyContainer sharedSecret)
        {
            return string.Format("{0}_{1}", sharedSecret.GetSecret(), Salt);
        }

        public static string Decrypt(this string message, EncryptionKeyContainer sharedSecret)
        {
            return Convert.FromBase64String(message).Decrypt(sharedSecret).GetStringFromArray(EncodingType.Unicode);
        }

        public static byte[] Decrypt(this byte[] file, EncryptionKeyContainer sharedSecret)
        {
            return file.RunCryptoService(sharedSecret, CreateDecryptTransform);
        }

        private static ICryptoTransform CreateDecryptTransform(TripleDESCryptoServiceProvider tdesAlgorithm)
        {
            return tdesAlgorithm.CreateDecryptor();
        }
    }
}