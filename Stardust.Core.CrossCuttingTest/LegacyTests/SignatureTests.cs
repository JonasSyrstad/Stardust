//
// SignatureTests.cs
// This file is part of Stardust.Core.CrossCuttingTest
//
// Author: Jonas Syrstad (jsyrstad2+StardustCore@gmail.com), http://no.linkedin.com/in/jonassyrstad/) 
// Copyright (c) 2013 Jonas Syrstad. All rights reserved.
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Core.Security;
using Stardust.Particles;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    [TestClass]
    public class SignatureTests
    {
        [TestMethod]
        [TestCategory("Security - Signing")]
        public void SignDocumentTest()
        {
            var file = GetFile();
            var privateKey = FileSigning.CreateNewPrivateCertificate();
            var publicKey = FileSigning.GetPublicCertificate(privateKey);
            var signedDoc = FileSigning.SignDocument(file, privateKey);
            Assert.IsTrue(FileSigning.Validate(file, publicKey, signedDoc));
        }

        [TestMethod]
        [TestCategory("Security - Signing")]
        public void EncryptDocument()
        {
            var file = GetFile();
            var encrypted = file.Encrypt(new EncryptionKeyContainer("secret"));
            var decryptedDoc = encrypted.Decrypt(new EncryptionKeyContainer("secret"));
            for (int i = 0; i < file.Length; i++)
            {
                Assert.AreEqual(file[i],decryptedDoc[i]);
            }
            //Assert.AreEqual(file, decryptedDoc);
        }

        private static byte[] GetFile()
        {
            var file = File.ReadAllBytes(@"C:\Users\Jonas\Pictures\iron maiden sancturay.jpg");
            return file;
        }
    }
}