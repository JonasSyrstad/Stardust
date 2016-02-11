//
// BinaryConverterFactoryTest.cs
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Clusters;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    
    
    /// <summary>
    ///This is a test class for BinaryConverterFactoryTest and is intended
    ///to contain all BinaryConverterFactoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BinaryConverterFactoryTest
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

        const string StringActual = "01 05 00 00 00 00 00 05 15 00 00 00 62 88 2B 8D 32 54 C5 1F C0 19 6E 75 76 04 00 00";

        readonly byte[] BinaryActual = new byte[]
                {
                    0x01, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x15, 0x00, 0x00, 0x00, 0x62, 0x88, 0x2B, 0x8D, 0x32,
                    0x54, 0xC5, 0x1F, 0xC0, 0x19, 0x6E, 0x75, 0x76, 0x04, 0x00, 0x00
                };

        

        [TestMethod()]
        [TestCategory("Binary converter")]
        public void ConvetHexStringToBinaryTest()
        {
            
            var hexConverter = BinaryConverterFactory.CreateConverter(ConverterTypes.Hex);
            
            var binarySid = hexConverter.StringToByteArray(StringActual);
            var index = 0;
            foreach (var b in binarySid)
            {
                Assert.AreEqual(BinaryActual[index],b);
                index++;
            }
        }

        [TestMethod]
        [TestCategory("Binary converter")]
        public void ConvetHexToStringTest()
        {
            var hexConverter = BinaryConverterFactory.CreateConverter(ConverterTypes.Hex);
            var stringSid = hexConverter.ByteArrayToString(BinaryActual, " ");
            Assert.AreEqual(StringActual,stringSid);
        }
    }
}
