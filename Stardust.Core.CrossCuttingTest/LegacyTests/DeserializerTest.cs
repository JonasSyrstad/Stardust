//
// DeserializerTest.cs
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
using Stardust.Core.CrossCuttingTest.LegacyTests.Mock;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;
using Stardust.Particles.Xml;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{


    /// <summary>
    ///This is a test class for DeserializerTest and is intended
    ///to contain all DeserializerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DeserializerTest
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

        string path = @"c:\temp\testfile.xml";
        string xml = "<?xml version=\"1.0\"?><NonSerializableObject xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://pragma.no/Pragma.Core.CrossCuttingTest.Mock.NonSerializableObject\">  <AnInt>12</AnInt><AString>test</AString></NonSerializableObject>";
        string notXml = "asdfasdfasdfasdfasdfasdf";
        
        [TestMethod()]
        [TestCategory("Serialization")]
        public void DeserializeTest()
        {
            var actual = Deserializer<NonSerializableObject>.Deserialize(xml,"http://pragma.no/Pragma.Core.CrossCuttingTest.Mock.NonSerializableObject");
            NonSerializableObject expected = new NonSerializableObject() { AnInt = 12, AString = "test" };
            Assert.AreEqual(expected.AString, actual.AString);
            Assert.AreEqual(expected.AnInt, actual.AnInt);
        }

        [TestMethod()]
        [TestCategory("Serialization")]
        public void TryGetFromFileSuccess()
        {
            xml.WriteToFile(path);
            NonSerializableObject result = null;
            NonSerializableObject expected = new NonSerializableObject() { AnInt = 12, AString = "test" };
            var actual = Deserializer<NonSerializableObject>.TryGetInstanceFromFile(path, ref result,"http://pragma.no/Pragma.Core.CrossCuttingTest.Mock.NonSerializableObject");
            Assert.IsNotNull(result);
            Assert.AreEqual(expected.AString, result.AString);
            Assert.AreEqual(expected.AnInt, result.AnInt);
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        [TestCategory("Serialization")]
        public void TryGetFromFileFailure()
        {
            NonSerializableObject result = null;
            notXml.WriteToFile(path);
            var actual = Deserializer<NonSerializableObject>.TryGetInstanceFromFile(path, ref result,"http://pragma.no/Pragma.Core.CrossCuttingTest.Mock.NonSerializableObject");
            Assert.IsNull(result);
            Assert.IsFalse(actual);
        }
    }
}
