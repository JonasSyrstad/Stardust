//
// PoolTests.cs
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
using Stardust.Core.Default.Implementations;
using Stardust.Core.Pool;
using Stardust.Dimensions;
using Stardust.Interstellar;
using Stardust.Interstellar.Serializers;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{

    [TestClass]
    public class PoolTests
    {
        private IKernelContext KernelScope;

        [TestInitialize]
        public void Initialize()
        {
            KernelScope = Resolver.BeginKernelScope();
            Resolver.LoadModuleConfiguration<TestBinding>();
            Resolver.GetConfigurator().UnBind<IReplaceableSerializer>().AllAndBind().To<JsonReplaceableSerializer>().SetSingletonScope().AllowOverride = false;
        }

        [TestCleanup]
        public void Cleanup()
        {
            Resolver.EndKernelScope(KernelScope, true);
        }

        [TestMethod]
        [TestCategory("Pooling")]
        public void CreateBasicPool()
        {
            PoolFactory.InitializePool<BasicTestPool>(2, null);
            var item1 = PoolFactory.Create<BasicTestPool>();
            var item2 = PoolFactory.Create<BasicTestPool>();
            Assert.AreNotEqual(item1, item2);
            item1.Dispose();
            item2.Dispose();
            PoolFactory.DisposePool<BasicTestPool>();
        }

        [TestMethod]
        [TestCategory("Pooling")]
        public async Task CreateConnectionStringBasedPool()
        {
            PoolFactory.InitializeNamedPool<TestPoolWithConnectionString>(2, null);
            var item1 = PoolFactory.Create<TestPoolWithConnectionString>("http://test1.com/mongo");
            var item2 = PoolFactory.Create<TestPoolWithConnectionString>("http://test2.com/mongo");
            var item3 = PoolFactory.Create<TestPoolWithConnectionString>("http://test1.com/mongo");
            var item4 = PoolFactory.Create<TestPoolWithConnectionString>("http://test2.com/mongo");

            Assert.AreNotEqual(item1, item2);
            Assert.AreNotEqual(item3, item4);
            Assert.AreNotEqual(item1, item3);
            Assert.AreNotEqual(item2, item3);
            Assert.AreNotEqual(item1, item4);
            try
            {
                var item = PoolFactory.Create<TestPoolWithConnectionString>("http://test1.com/mongo");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(Exception));
            }

            var itemTask = Task.Run(()=>PoolFactory.CreateAsync<TestPoolWithConnectionString>("http://test1.com/mongo"));
            item1.Dispose();
            await Task.Delay(1);
            var item5 = await itemTask.ConfigureAwait(false);

            item2.Dispose();
            item3.Dispose();
            item4.Dispose();
            item5.Dispose();
            try
            {
                PoolFactory.DisposeNamedPool<TestPoolWithConnectionString>();
            }
            catch
            { }
        }
    }

    public class TestBinding : Blueprint
    {
        protected override void DoCustomBindings()
        {
        }
    }

    internal class TestPoolWithConnectionString : ConnectionStringPoolableBase
    {
        protected override void Dispose(bool disposing)
        {
        }
    }

    internal class BasicTestPool : PoolableBase
    {
        protected override void Dispose(bool disposing)
        {
        }
    }
}