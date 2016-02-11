//
// RuntimeHelperTest.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Core.Wcf;
using Stardust.Interstellar;
using Stardust.Interstellar.Trace;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;
using Stardust.Particles.Xml;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{


    /// <summary>
    ///This is a test class for RuntimeHelperTest and is intended
    ///to contain all RuntimeHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RuntimeHelperTest
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
        private static string result;

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
        public async Task CreateContextScopeContainer()
        {
            using (var scopeManager = RequestResponseScopefactory.CreateScope())
            {
                var runtime = RuntimeFactory.CreateRuntime();
                Assert.AreEqual(scopeManager.ContextId, runtime.InstanceId);
            }
        }

        [TestMethod()]
        [TestCategory("Containers")]
        public async Task CreateContextScopeContainerAndUseInAsyncSenario()
        {
            result = "....";
            using (var scopeManager = RequestResponseScopefactory.CreateScope())
            {
                var runtime = RuntimeFactory.CreateRuntime();
                Assert.AreEqual(scopeManager.ContextId, runtime.InstanceId);
                var value=await DoSomethingAsync("Meshuggah");
                Assert.AreEqual("Meshuggah",value);
            }
            Assert.AreEqual("Meshuggah",result);
            using (var scopeManager = RequestResponseScopefactory.CreateScope())
            {
                using (var t= TracerFactory.StartTracer(this, "CreateContextScopeContainerAndUseInAsyncSenario"))
                {
                    var runtime = RuntimeFactory.CreateRuntime();
                    Assert.AreEqual(scopeManager.ContextId, runtime.InstanceId);
                    var value = await DoSomethingAsync("Iron Maiden");
                    Assert.AreEqual("Iron Maiden", value);
                    Console.WriteLine(t.GetCallstack().Serialize(forceSerializationAnyway:true));
                }
            }
            Assert.AreNotEqual("Meshuggah", result);
        }

        private async Task<string> DoSomethingAsync(string meshuggah)
        {
            using (TracerFactory.StartTracer(this, "DoSomethingAsync"))
            {
                Console.WriteLine(meshuggah);
                await AndIntoAStatic(meshuggah);
                await Task.Delay(10);
                return meshuggah;
            }
        }

        private static Task AndIntoAStatic(string meshuggah)
        {
            using (var t=TracerFactory.StartTracer(meshuggah, "AndIntoAStatic"))
            {
                result = meshuggah;
                t.SetAdidtionalInformation(meshuggah);
                return Task.Delay(10);
            }
        }

        /// <summary>
        ///A test for GetBinFolderLocation
        ///</summary>
        [TestMethod()]
        [TestCategory("Runtime helper")]
        public void GetApplicationTypeTest()
        {
            var appType = RuntimeHelper.GetProcessName();
            var expected = new[] { "QTAgent32", "vstest.executionengine.x86" };
            Assert.IsTrue(expected.Contains(appType));
        }

        [TestMethod]
        [TestCategory("Runtime helper")]
        public void GetMemoryUsageTest()
        {
            var memoryUsage = RuntimeHelper.GetCurrentMemoryUsage();
            Assert.AreNotEqual(0, memoryUsage);
        }

        [TestMethod]
        [TestCategory("Runtime helper")]
        public void GetProcessRuntimeTest()
        {
            var runtime = RuntimeHelper.GetApplicationUptime();
            Assert.IsTrue(runtime.Ticks > 0);
        }

        [TestMethod]
        [TestCategory("Runtime helper")]
        public void GetPercentTimeInGC()
        {
            GC.Collect(2);
            var timeInGC = RuntimeHelper.GetTimeInGC();
            Assert.IsTrue(timeInGC > 0);
        }

        [TestMethod]
        [TestCategory("Runtime helper")]
        public void GetCurrentCpuUsageTest()
        {
            var currentCpu = RuntimeHelper.GetCurrentCpuUsage();
            Assert.IsTrue(currentCpu >= 0);

        }

        //[TestMethod]
        public void GetLargeObjectHeapSizeTest()
        {

            var sb = new StringBuilder();
            for (var i = 0; i < 40000; i++)
            {
                sb.Append("123123123123123123123123123123123123123123123412341234123412341234123412341234asdfasdfasdfasdfasdfasdf");
            }
            var shouldEndUpInLoh = sb.ToString();
            var largeObjectHeap = RuntimeHelper.GetLargeObjectHeapSize();
            Assert.IsTrue(largeObjectHeap >= shouldEndUpInLoh.Length);
        }
    }
}
