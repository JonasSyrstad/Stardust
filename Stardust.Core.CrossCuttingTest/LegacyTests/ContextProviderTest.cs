//
// ContextProviderTest.cs
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Core.Wcf;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{


    /// <summary>
    ///This is a test class for ContextProviderTest and is intended
    ///to contain all ContextProviderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ContextProviderTest
    {
        private const string ThisIsATest = "this is a test";


        private TestContext testContextInstance;
        private Guid second;

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

        [TestMethod()]
        [TestCategory("Containers")]
        public void BindInstanceToSessionTest()
        {
            var c = ContainerFactory.Current;
            c.Bind(typeof(Implementation), new Implementation(), Scope.Singleton);
            var actual = (Implementation)c.Resolve(typeof(Implementation), Scope.Singleton);
            Assert.IsTrue(actual.IsInstance());
        }


        

        [TestMethod()]
        [TestCategory("Containers")]
        public async Task CustomScopeContextHandler()
        {
            second = Guid.NewGuid();
            try
            {
                Guid main;
                using (var maincs = ThreadSynchronizationContext.BeginContext() as ThreadSynchronizationContext)
                {
                    Console.WriteLine("managed thread id {0}", Thread.CurrentThread.ManagedThreadId);
                    Console.WriteLine(maincs.Id.ToString());
                    main = maincs.Id;
                    var myvalue = await CreateTestTask();
                    Console.WriteLine("managed thread id {0}", Thread.CurrentThread.ManagedThreadId);
                    Console.WriteLine(maincs.Id.ToString());
                    Assert.AreEqual("myNewItem", myvalue);
                }
                Assert.AreEqual(main, second);
            }
            catch (Exception ex)
            {
                Assert.Fail();

            }
        }

        private Task<string> CreateTestTask()
        {
            return NestedTask();
        }

        private async Task<string> NestedTask()
        {
            await Task.Delay(10);
            Console.WriteLine("managed thread id {0}", Thread.CurrentThread.ManagedThreadId);
            var sc = SynchronizationContext.Current as ThreadSynchronizationContext;
            Assert.IsInstanceOfType(SynchronizationContext.Current, typeof (ThreadSynchronizationContext));
            Console.WriteLine(sc.Id.ToString());
            second = sc.Id;
            return await Task.FromResult("myNewItem");
        }

        [TestMethod()]
        [TestCategory("Containers")]
        public void ExtendedScopeProviderSingleton()
        {
            var c = ContainerFactory.Current;
            ContainerFactory.Current.KillAllInstances();
            Implementation actual;
            c.Bind(typeof(string), ThisIsATest, Scope.Singleton);
            using (c.ExtendScope(Scope.Singleton))
            {
                c.Bind(typeof(Implementation), new Implementation(), Scope.Singleton);
                actual = (Implementation)c.Resolve(typeof(Implementation), Scope.Singleton);
                var actualInherited = c.Resolve(typeof(string), Scope.Singleton);
                Assert.AreEqual(ThisIsATest, actualInherited);
                Assert.IsTrue(actual.IsInstance());
            }
            actual = (Implementation)c.Resolve(typeof(Implementation), Scope.Singleton);
            Assert.IsNull(actual);
        }
        [TestMethod()]
        [TestCategory("Containers")]
        public void ExtendedScopeProviderThread()
        {
            var c = ContainerFactory.Current;
            ContainerFactory.Current.KillAllInstances();
            Implementation actual;
            c.Bind(typeof(string), ThisIsATest, Scope.Thread);
            using (c.ExtendScope(Scope.Thread))
            {
                c.Bind(typeof(Implementation), new Implementation(), Scope.Thread);
                actual = (Implementation)c.Resolve(typeof(Implementation), Scope.Thread);
                var actualInherited = c.Resolve(typeof(string), Scope.Thread);
                Assert.AreEqual(ThisIsATest, actualInherited);
                Assert.IsTrue(actual.IsInstance());
            }
            actual = (Implementation)c.Resolve(typeof(Implementation), Scope.Thread);
            Assert.IsNull(actual);
        }


    }
}
