//
// TypesetHelperTest.cs
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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Core.CrossCuttingTest.LegacyTests.Mock;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    /// <summary>
    ///This is a test class for TypesetHelperTest and is intended
    ///to contain all TypesetHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TypesetHelperTest
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

        #endregion Additional test attributes

        /// <summary>
        ///A test for FindSubTypeOf
        ///</summary>
        [TestMethod()]
        [TestCategory("Assembly scanner")]

        [Ignore] //Assembly scanning not implemented yet
        public void FindSubTypeOfTest()
        {
            Type baseType = typeof(Interface1);
            string named = "NonSerializableObject";
            object expected = typeof(NonSerializableObject);
            object actual;

            var firstRunStart = DateTime.Now;
            actual = Resolver.Activate(baseType, named);
            var firstRunEnd = DateTime.Now;
            Assert.AreEqual(expected, actual.GetType());

            var lastRunStart = DateTime.Now;
            actual = Resolver.Activate(baseType, named);
            var lastRunEnd = DateTime.Now;
            Assert.IsTrue(CompaireRunTime(firstRunEnd - firstRunStart, lastRunEnd - lastRunStart));
            Assert.AreEqual(expected, actual.GetType());
        }

        [TestMethod]
        [TestCategory("Assembly scanner")]
        public void TestResolver()
        {
            Resolver.GetConfigurator().Bind<ITestInterface<long>>().To<TestInterface<long>>().SetRequestResponseScope();
            Resolver.GetConfigurator().Bind<string>().To<string>().SetRequestResponseScope();
            Resolver.GetConfigurator().Bind<Int32>().To<Int32>().SetRequestResponseScope();
            ContainerFactory.Current.Bind(typeof(string),"test",Scope.Context);
            ContainerFactory.Current.Bind(typeof(Int32), 10, Scope.Context);
            var actual = Resolver.Activate<ITestInterface<long>>() as TestInterface<long>;
            var expected = new TestInterface<long>("test", 10);
            Assert.AreEqual(expected.Test, actual.Test);
            Assert.AreEqual(expected.Test2, actual.Test2);
        }

        private static bool CompaireRunTime(TimeSpan firstRun, TimeSpan lastRun)
        {
            return lastRun < firstRun;
        }
    }
}