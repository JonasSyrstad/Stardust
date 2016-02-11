//
// ModuleCreatorConfiguratorTest.cs
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
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    public interface IConfiguredInstance
    {
    }

    /// <summary>
    ///This is a test class for ModuleCreatorConfiguratorTest and is intended
    ///to contain all ModuleCreatorConfiguratorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ModuleCreatorConfiguratorTest
    {

        private TestContext testContextInstance;
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

        //[TestMethod()]
        //[TestCategory("Resolver")]
        //public void BindTest()
        //{
        //    Resolver.UnBind<Interface1>().All();
        //    var expected = typeof(Implementation);
        //    Resolver.Bind<Interface1>()
        //        .To<Implementation>("test");
        //    var actual = Resolver.Resolve<Interface1>("test")
        //        .Activate();
        //    Assert.IsInstanceOfType(actual, expected);
        //    Resolver.UnBind<Interface1>().All();
        //}

        //[TestMethod()]
        //[TestCategory("Resolver")]
        //public void UnbindAllAndBindToTest()
        //{
        //    Resolver.UnBind<Interface1>().All();
        //    Resolver.Bind<Interface1>()
        //        .To<Implementation>().SetTransientScope();
        //    var item = Resolver.Resolve<Interface1>();
        //    Assert.AreEqual(typeof(Implementation), item.TypeContext.BoundType);
        //    Resolver.UnBind<Interface1>().AllAndBind().To<Implementation2>().SetTransientScope();
        //    item = Resolver.Resolve<Interface1>();
        //    Assert.AreEqual(typeof(Implementation2), item.TypeContext.BoundType);
        //    Resolver.UnBind<Interface1>().All();
        //}


        //[TestMethod()]
        //[TestCategory("Resolver")]
        //public void Resolve_DoesNotExsistTest()
        //{
        //    var expected = typeof(ModuleNotFoundException);

        //    try
        //    {
        //       var resolvedType =Resolver.ResolveType<Interface1>("aslkdfj");
        //        Assert.IsNull(resolvedType);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.IsInstanceOfType(ex, expected);
        //    }
        //}

        [TestMethod()]
        [TestCategory("Resolver")]
        public void ResolveFromConfigSection()
        {
            var defaultInstance = Resolver.Activate<IConfiguredInstance>();
            var defaultInstance2 = Resolver.Activate<IConfiguredInstance>();
            var alternateInstance = Resolver.Activate<IConfiguredInstance>("fromConfig");
            Assert.IsInstanceOfType(defaultInstance, typeof(DefaultInstance));
            Assert.AreSame(defaultInstance,defaultInstance2);
            Assert.IsInstanceOfType(alternateInstance, typeof(AlternateInstance));
        }
    }

    public class DefaultInstance : IConfiguredInstance
    {
    }

    public class Implementation : Interface1
    { }

    internal class AlternateInstance : IConfiguredInstance
    {
    }
}