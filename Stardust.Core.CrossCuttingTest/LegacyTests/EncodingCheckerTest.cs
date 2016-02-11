//
// EncodingCheckerTest.cs
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
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Clusters;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    /// <summary>
    ///This is a test class for EncodingCheckerTest and is intended
    ///to contain all EncodingCheckerTest Unit Tests
    ///</summary>
    [TestClass]
    public class EncodingCheckerTest
    {
        private IKernelContext KernelScope;

        [TestInitialize]
        public void Initialize()
        {
            KernelScope = Resolver.BeginKernelScope();
            Resolver.LoadModuleConfiguration<Blueprint>();

        }

        [TestCleanup]
        public void Cleanup()
        {
            Resolver.EndKernelScope(KernelScope, true);
        }

        const string ExpectedContent = "Jonas syrstad, Os i Østerdalen";

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

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
        ///A test for Check
        ///</summary>
        [TestMethod]
        [TestCategory("Encoding checker")]
        [DeploymentItem("TestFiles/utf8TestFile.txt")]
        public void CheckUtf8Test()
        {
            var filePath = @"utf8TestFile.txt";
            var expected = Encoding.UTF8;
            var actual = EncodingFactory.Check(filePath);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(ExpectedContent, EncodingFactory.ReadFileText(filePath));
        }

        [TestMethod]
        [TestCategory("Encoding checker")]
        [DeploymentItem("TestFiles/ansiTestFile.txt")]
        public void CheckAnsiTest()
        {
            var filePath = @"ansiTestFile.txt";
            var expected = Encoding.Default;
            var actual = EncodingFactory.Check(filePath);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(ExpectedContent, EncodingFactory.ReadFileText(filePath));
        }

        [TestMethod]
        [TestCategory("Encoding checker")]
        [DeploymentItem("TestFiles/unicodeTestFile.txt")]
        public void CheckUnicodeTest()
        {
            var filePath = @"unicodeTestFile.txt";
            var expected = Encoding.Unicode;
            var actual = EncodingFactory.Check(filePath);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(ExpectedContent, EncodingFactory.ReadFileText(filePath));
        }

        [TestMethod]
        [TestCategory("Encoding checker")]
        [DeploymentItem("TestFiles/bigEndianUnicodeTestFile.txt")]
        public void CheckUnicodeBigendianTest()
        {
            var filePath = @"bigEndianUnicodeTestFile.txt";
            var actual = EncodingFactory.Check(filePath);
            var content = actual.GetString(File.ReadAllBytes(filePath));
            Assert.AreEqual(ExpectedContent, content.RemoveFileMarkers());
        }
    }
}
