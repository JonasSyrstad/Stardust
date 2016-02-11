//
// EncodingFactoryTest.cs
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
    ///This is a test class for EncodingFactoryTest and is intended
    ///to contain all EncodingFactoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EncodingFactoryTest
    {
        private IKernelContext KernelScope;

        [TestInitialize]
        public void Initialize()
        {
            ContainerFactory.Current.KillAllInstances();
            KernelScope = Resolver.BeginKernelScope();
            Resolver.LoadModuleConfiguration<Blueprint>();

        }

        [TestCleanup]
        public void Cleanup()
        {
            Resolver.EndKernelScope(KernelScope, true);
        }

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
        ///A test for Check
        ///</summary>
        [TestMethod()]
        [TestCategory("Encoding checker")]
        [DeploymentItem("TestFiles/utf8TestFile.txt")]
        public void CheckTest()
        {
            Stream stream =
                File.OpenRead(@"utf8TestFile.txt");
            var expected = Encoding.UTF8; // TODO: Initialize to an appropriate value
            var actual = EncodingFactory.Check(stream);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Check
        ///</summary>
        [TestMethod()]
        [TestCategory("Encoding checker")]
        [DeploymentItem("TestFiles/utf8TestFile.txt")]
        public void CheckTest1()
        {
            var fileName =@"utf8TestFile.txt";
            var expected = Encoding.UTF8;
            var actual = EncodingFactory.Check(fileName);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Check
        ///</summary>
        [TestMethod()]
        [TestCategory("Encoding checker")]
        [DeploymentItem("TestFiles/utf8TestFile.txt")]
        public void CheckTest2()
        {
            byte[] buffer = File.ReadAllBytes(@"utf8TestFile.txt");
            var expected = Encoding.UTF8;
            var actual = EncodingFactory.Check(buffer, buffer.Length);
            Assert.AreEqual(expected, actual);
        }

        const string ExpectedContent = "Jonas syrstad, Os i Østerdalen";
        [TestMethod]
        [TestCategory("Encoding checker")]
        public void ReadTextFromStreamTest()
        {
            Stream stream =
               File.OpenRead(@"utf8TestFile.txt");
            var text=EncodingFactory.ReadFileText(stream);
            Assert.AreEqual(ExpectedContent,text);
        }
    }
}
