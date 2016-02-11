//
// ByteArrayExtenderTest.cs
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
using Stardust.Particles;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{


    /// <summary>
    ///This is a test class for ByteArrayExtenderTest and is intended
    ///to contain all ByteArrayExtenderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ByteArrayExtenderTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for GetStringFromArray
        ///</summary>
        [TestMethod()]
        [TestCategory("Encoding")]
        public void GetStringFromUtf8ArrayTest()
        {
            string expected = "Test!";
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            var self = encoding.GetBytes(expected);
            var actual = self.GetStringFromArray(EncodingType.Utf8);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetStringFromUtf7Array
        ///</summary>
        [TestMethod()]
        [TestCategory("Encoding")]
        public void GetStringFromUtf7ArrayTest()
        {
            string expected = "Test!";
            System.Text.UTF7Encoding encoding = new System.Text.UTF7Encoding();
            var self = encoding.GetBytes(expected);
            var actual = self.GetStringFromArray(EncodingType.Utf7);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetStringFromUnicodeArray
        ///</summary>
        [TestMethod()]
        [TestCategory("Encoding")]
        public void GetStringFromUnicodeArrayTest()
        {
            string expected = "Test!";
            System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding();
            var self = encoding.GetBytes(expected);
            var actual = self.GetStringFromArray(EncodingType.Unicode);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetStringFromAsciiArray
        ///</summary>
        [TestMethod()]
        [TestCategory("Encoding")]
        public void GetStringFromAsciiArrayTest()
        {
            string expected = "Test!";
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            var self = encoding.GetBytes(expected);
            var actual = self.GetStringFromArray(EncodingType.Ascii);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Reverse
        ///</summary>
        [TestMethod()]
        [TestCategory("Encoding")]
        public void ReverseTest()
        {
            byte[] self = { 1, 2, 3, 4 }; // TODO: Initialize to an appropriate value
            byte[] expected = { 4, 3, 2, 1 }; // TODO: Initialize to an appropriate value
            byte[] actual = self.Reverse();
            Assert.AreEqual(expected[0], actual[0]);
            Assert.AreEqual(expected[1], actual[1]);
            Assert.AreEqual(expected[2], actual[2]);
            Assert.AreEqual(expected[3], actual[3]);
        }

        /// <summary>
        ///A test for Compress
        ///</summary>
        [TestMethod()]
        [TestCategory("Encoding")]
        public void CompressTest()
        {
            var self = File.ReadAllBytes(@"C:\Private projects\Core Framework\Stardust\Stardust.Core.CrossCuttingTest\TestFiles\OldExcelTestFile.xls");
            var actual= self.Compress();
            var decompressed = actual.Decompress();
            Assert.IsTrue(self.Length>actual.Length);
            for (var i = 0; i < self.Length; i++)
            {
                Assert.AreEqual(self[i],decompressed[i]);
            }
        }
    }
}
