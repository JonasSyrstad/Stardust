//
// BundActivatorTests.cs
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
using Stardust.Nucleus.ObjectActivator;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    [TestClass()]
    public class BoundActivatorTests
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
        [TestCategory("Bound Activator")]
        public void CreateFullyInitializedObject()
        {
            Resolver.GetConfigurator().Bind<IBoundAttribTest>().To<BoundAttribTest>();
            ActivatorFactory.SetBoundActivator();
            var activator = ActivatorFactory.Activator;
            var actual = activator.Activate<IBoundTest>(typeof(BoundTest));
            Assert.IsNotNull(actual.BoundAttribute);
            Assert.IsNotNull(actual.TestGetter);
            Assert.IsInstanceOfType(actual.BoundAttribute, typeof(BoundAttribTest));
            Assert.IsInstanceOfType(actual.TestGetter, typeof(BoundAttribTestAnotherOne));
            var sameTwice = actual.TestGetter;
            //Run the whole thing once more to test cache and scope settings.
            activator = ActivatorFactory.Activator;
            actual = activator.Activate<IBoundTest>(typeof(BoundTest));
            Assert.IsNotNull(actual.BoundAttribute);
            Assert.IsNotNull(actual.TestGetter);
            Assert.IsInstanceOfType(actual.BoundAttribute, typeof(BoundAttribTest));
            Assert.IsInstanceOfType(actual.TestGetter, typeof(BoundAttribTestAnotherOne));
            Assert.AreSame(sameTwice,actual.TestGetter);
            ActivatorFactory.ResetActivator();
        }

        [TestMethod]
        [TestCategory("Bound Activator")]
        public void CreateOnlyOne()
        {
            Resolver.GetConfigurator().Bind<IBoundAttribTest>().To<BoundAttribTest>().SetSingletonScope().DisableOverride();
            ActivatorFactory.SetBoundActivator();
            var actual1 = ActivatorFactory.Activator.Activate<IBoundTest>(typeof(BoundTestOnlyOneInstance));
            var actual2 = ActivatorFactory.Activator.Activate<IBoundTest>(typeof(BoundTestOnlyOneInstance));
            Assert.AreSame(actual1.BoundAttribute,actual2.BoundAttribute);
            ActivatorFactory.ResetActivator();
        }

        [TestMethod]
        [TestCategory("Bound Activator")]
        public void CreateNewInstanceIthConstructorParameters()
        {
            Resolver.GetConfigurator().Bind<IConstructorAttributeTest>().To<ConstructorAttributeTest>();
            Resolver.GetConfigurator().Bind<IConstructorParameterTest>().To<ConstructorParameterTest>();
            var item = Resolver.Activate<IConstructorParameterTest>();
            Assert.IsNotNull(item);
            Assert.IsNotNull(item.MyConstructorAttribute);
        }
    }

    public class BoundTest : IBoundTest
    {
        [Bound]
        public IBoundAttribTest BoundAttribute { get; set; }

        [Bound(typeof(BoundAttribTestAnotherOne),Scope.Singleton)]
        private IBoundAttribTest Bound;

        public IBoundAttribTest TestGetter { get { return Bound; } }
    }

    public interface IBoundAttribTest
    { }


    class BoundAttribTestAnotherOne : IBoundAttribTest
    {
    }

    public class BoundAttribTest : IBoundAttribTest
    {
    }

    public interface IConstructorParameterTest
    {
        IConstructorAttributeTest MyConstructorAttribute { get; }
    }

    class ConstructorParameterTest : IConstructorParameterTest
    {
        public ConstructorParameterTest(IConstructorAttributeTest value)
        {
            MyConstructorAttribute = value;
        }

        public IConstructorAttributeTest MyConstructorAttribute { get; private set; }
    }

    public interface IConstructorAttributeTest
    { }

    class ConstructorAttributeTest : IConstructorAttributeTest
    {
    }
}
