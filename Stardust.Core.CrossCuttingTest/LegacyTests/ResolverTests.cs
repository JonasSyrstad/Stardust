//
// ResolverTests.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    [TestClass]
    public class ResolverTests
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

        [TestMethod]
        [TestCategory("Resolver configuration")]
        public void RestModuleConfiguration_GetByName()
        {
            Resolver.RemoveAll();
            Resolver.LoadModuleConfiguration(new ConfigurationSettings());
            var expected = typeof(ModuleConfigTest2);
            var actual = Resolver.Activate<IModuleConfigTest>("test2");
            Assert.AreEqual(expected, actual.GetType());
        }

        [TestMethod]
        [TestCategory("Resolver configuration")]
        public void RestModuleConfiguration_GetByEnum()
        {
            Resolver.RemoveAll();
            Resolver.LoadModuleConfiguration(new ConfigurationSettings());
            var expected = typeof(ModuleConfigTest2);
            var actual = Resolver.Activate<IModuleConfigTest>(ConsoleKey.Add);
            Assert.AreEqual(expected, actual.GetType());
        }

        [TestMethod]
        [TestCategory("Resolver configuration")]
        public void RestModuleConfiguration_GetDefault()
        {
            Resolver.GetConfigurator().RemoveAll();
            Resolver.LoadModuleConfiguration(new ConfigurationSettings());
            var expected = typeof(ModuleConfigTest);
            var actual = Resolver.Activate<IModuleConfigTest>();
            Assert.AreEqual(expected, actual.GetType());

        }

        [TestMethod]
        [TestCategory("Resolver configuration")]
        public void CreateInstanceWithoutGenericParameterFunc()
        {
            Resolver.GetConfigurator().Bind<IDictionary<string, int>>().To<Dictionary<string, int>>();
            var actual = Resolver.Activate<IDictionary<string, int>>();
            Assert.IsInstanceOfType(actual, typeof(IDictionary<string, int>));
        }

        [TestMethod]
        [TestCategory("Resolver configuration")]
        public void CreateInstanceUnboundGenericParameterFunc()
        {
            Resolver.GetConfigurator().BindAsGeneric(typeof(IEnumerable<>)).To(typeof(List<>));
            var list = Resolver.ActivateGeneric<IEnumerable<int>>();
            ((List<int>)list).Add(1);
            Assert.IsInstanceOfType(list,typeof(List<int>));
            Assert.AreEqual(1, list.Count());
        }

        [TestMethod]
        [TestCategory("Resolver configuration")]
        public void CreateInstanceTransientLoadTest()
        {
            var target = 1600;
            Resolver.GetConfigurator().Bind<IModuleConfigTest>().To<ModuleConfigTest>().SetTransientScope();
            var warmup = Resolver.Activate<IModuleConfigTest>();
            int iterations = 1000000;
            var timer = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var instance = Resolver.Activate<IModuleConfigTest>();

            }
            timer.Stop();
            var ms = timer.Elapsed.TotalMilliseconds;
            if (ms > target)
                Assert.Fail("Test ran for more than {1} ms: total ms 1M transient resolves: {0}", ms, target);
        }

        [TestMethod]
        [TestCategory("Resolver configuration")]
        public void CreateInstanceSingletonLoadTest()
        {
            var target = 700;
            Resolver.GetConfigurator().Bind<IModuleConfigTest>().To<ModuleConfigTest>().SetSingletonScope();
            var warmup = Resolver.Activate<IModuleConfigTest>();
            ContainerFactory.Current.KillAllInstances();
            int iterations = 1000000;
            var timer = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var instance = Resolver.Activate<IModuleConfigTest>();

            }
            timer.Stop();
            var ms = timer.Elapsed.TotalMilliseconds;
            if(ms>target)
                Assert.Fail("Test ran for more than {1} ms: total ms 1M singleton resolves: {0}", ms,target);
        }

        [TestMethod]
        [TestCategory("Resolver configuration")]
        public void ResolveUnboundInterfaceTest()
        {
            var actual = Resolver.Activate<IUnbound>();
            Assert.IsNull(actual);
        }

        public class ConfigurationSettings : IBlueprint
        {


            public void Bind(IConfigurator resolver)
            {
                resolver.Bind<IModuleConfigTest>().To<ModuleConfigTest>();
                resolver.Bind<IModuleConfigTest>().To<ModuleConfigTest2>("test2");
                resolver.Bind<IModuleConfigTest>().To<ModuleConfigTest2>(ConsoleKey.Add);
                
            }

            public Type LoggingType
            {
                get { return typeof (LoggingDefaultImplementation); }
            }
        }

        public interface IModuleConfigTest
        {

        }
        public class ModuleConfigTest : IModuleConfigTest
        {
        }

        public class ModuleConfigTest2 : IModuleConfigTest
        {
        }
    }

    public interface IUnbound

    {
    }
}
