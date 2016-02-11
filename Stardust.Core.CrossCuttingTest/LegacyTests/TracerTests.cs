//
// TracerTests.cs
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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Core.Wcf;
using Stardust.Dimensions;
using Stardust.Interstellar;
using Stardust.Interstellar.Trace;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    [TestClass]
    public class TracerTests
    {
        private IKernelContext KernelScope;

        [TestInitialize]
        public void Initialize()
        {
            KernelScope = Resolver.BeginKernelScope();
            Resolver.LoadModuleConfiguration<Blueprint>();
            Scope = RequestResponseScopefactory.CreateScope();

        }

        public IStardustContext Scope { get; set; }

        [TestCleanup]
        public void Cleanup()
        {
            Scope.Dispose();
            Resolver.EndKernelScope(KernelScope, true);
        }

        [TestMethod]
        [TestCategory("Tracer")]
        public void SimpleTraceTest()
        {
            ITracer tracer;
            using (tracer = TracerFactory.StartTracer(this, "Test", "Test"))
            {
                using (TracerFactory.StartTracer(this, "Inner1", "test"))
                {
                    var item = new InnerTracerTest();
                    item.SomeMethod();
                }
                using (TracerFactory.StartTracer(this, "Inner2", "test"))
                {
                    var item = new InnerTracerTest();
                    item.SomeMethod();
                }
                Assert.IsNull(tracer.ParentItem);
                Assert.IsNotNull(tracer.GetCallstack());
                Assert.IsNotNull(tracer.GetCallstack().CallStack.FirstOrDefault());
            }
        }

        [TestMethod]
        [TestCategory("Tracer")]
        public async Task SimpleTraceAsyncTest()
        {
            try
            {
                ITracer tracer;
                using (tracer = TracerFactory.StartTracer(this, "Test", "Test"))
                {
                    InnerTracerTest item = null;
                    using (TracerFactory.StartTracer(this, "Inner2", "test"))
                    {
                        item = new InnerTracerTest();
                        item.SomeMethod();
                        await item.LoopAsync();
                        await item.SomeMethodAsync();
                        try
                        {
                            item.SomeMethodFail();
                            Debug.WriteLine("What?");
                        }
                        catch (AggregateException agex)
                        {
                            Assert.IsInstanceOfType(agex.InnerException,typeof(NotImplementedException));
                        }
                        catch (Exception ex)
                        {
                            var e = ex;
                            Assert.IsInstanceOfType(ex, typeof(NotImplementedException));
                        }
                        try
                        {
                             await item.SomeMethodFailAsync();
                        }
                        catch (AggregateException agex)
                        {
                            Assert.IsInstanceOfType(agex.InnerException, typeof(NotImplementedException));
                        }
                        catch (Exception ex)
                        {
                            var e = ex;
                            Assert.IsInstanceOfType(ex, typeof(NotImplementedException));
                        }
                        var t = item.NotAwaited();
                        Assert.IsFalse(item.asyncSet);
                        await item.SomeMethodAsync();
                        await t;
                        Assert.IsTrue(item.asyncSet);
                    }
                    Assert.IsNull(tracer.ParentItem);
                    Assert.IsNotNull(tracer.GetCallstack());
                    Assert.IsNotNull(tracer.GetCallstack().CallStack.FirstOrDefault());
                    Assert.IsTrue(item.asyncFailedCalled);
                    Assert.IsTrue(item.syncFailedCalled);
                }
            }
            catch (Exception ex)
            {
                ex.Log();
                throw;
            }
        }
    }

    public class InnerTracerTest
    {
        internal bool syncFailedCalled;

        internal bool asyncFailedCalled;

        internal bool asyncSet;

        public InnerTracerTest()
        {
            using (TracerFactory.StartTracer(this, "ctor"))
            {
            }
        }

        [Trace]
        public void SomeMethod()
        {
            
        }

        [Trace]
        public async Task<int> SomeMethodAsync()
        {
            await Task.Delay(100);
            return 1;
        }

        [Trace]
        public async Task LoopAsync()
        {
            await Function();
        }

        [Trace]
        private async Task Function()
        {
            for (var i = 0; i < 5; i++)
            {
                await SomeMethodAsync();
            }
        }

        [Trace]
        public void SomeMethodFail()
        {
            syncFailedCalled = true;
            throw new NotImplementedException();
        }

        [Trace]
        public async Task SomeMethodFailAsync()
        {
            await Task.Delay(100);
            asyncFailedCalled = true;
            //return 100;
            throw new NotImplementedException();
        }

        [Trace]
        public Task NotAwaited()
        {
            return Task.Delay(1000).ContinueWith(e => { asyncSet = true; });

        }
    }
}
