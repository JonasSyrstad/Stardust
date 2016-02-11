//
// ModuleRegistrationFactoryTest.cs
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

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Clusters;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.Extensions;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    
    
    /// <summary>
    ///This is a test class for ModuleRegistrationFactoryTest and is intended
    ///to contain all ModuleRegistrationFactoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ModuleRegistrationFactoryTest
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
        [TestMethod()]
        [TestCategory("Module registration")]
        public void ImportAssemblyWithTest()
        {
            CleanTestFolder();
            var expected = "ILoggingDummyImp.Dummy";
            var assembly = File.ReadAllBytes(@"C:\Temp\Version1\ILoggingDummyImp.dll");
            ModuleRegistrationFactory.Initialize(@"C:\Temp\Modules");
            ModuleRegistrationFactory.ReEnumerate();
            ModuleRegistrationFactory.ImportAssemblyWith<ILogging>("Test", assembly);
            var actual = ModuleRegistrationFactory.GetObjectInitializerFor<ILogging>("Test");
            Assert.AreEqual(expected,actual.FullName);
            
        }

        private void CleanTestFolder()
        {
            try
            {
                Directory.Delete(@"C:\Temp\Modules\ILoggingDummyImp_Version=1.0.0.0_Culture=neutral_PublicKeyToken=null",true);
                File.Delete(@"C:\Temp\Modules\moduleStore.xml");
                Directory.Delete(@"C:\Temp\Modules\ILoggingDummyImp_Version=2.0.0.0_Culture=neutral_PublicKeyToken=null", true);
            }
            catch
            {
            }
        }

        [TestMethod()]
        [TestCategory("Module registration")]
        public void UpdateAssemblyVersionInStoreTest()
        {
            CleanTestFolder();
            var expected = "ILoggingDummyImp.Dummy";
            var assembly = File.ReadAllBytes(@"C:\Temp\Version1\ILoggingDummyImp.dll");
            ModuleRegistrationFactory.Initialize(@"C:\Temp\Modules");
            ModuleRegistrationFactory.ReEnumerate();
            ModuleRegistrationFactory.ImportAssemblyWith<ILogging>("Test", assembly);
            var actual = ModuleRegistrationFactory.GetObjectInitializerFor<ILogging>("Test");
            Assert.AreEqual(expected, actual.FullName);
            ModuleRegistrationFactory.ReEnumerate();
            assembly = File.ReadAllBytes(@"C:\Temp\Version2\ILoggingDummyImp.dll");
            ModuleRegistrationFactory.ImportAssemblyWith<ILogging>("Test", assembly);
            actual = ModuleRegistrationFactory.GetObjectInitializerFor<ILogging>("Test");
            Assert.AreEqual("ILoggingDummyImp, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null", actual.AssemblyName);
        }

        [TestMethod()]
        [TestCategory("Module registration")]
        public void GetInstanceTest() 
        {
            GetInstanceTest_Helper();
            var instance = ModuleRegistrationFactory.GetInstance<ILogging>("Test");
            Assert.IsTrue(instance.GetType().Implements(typeof(ILogging)));

        }

        private static void GetInstanceTest_Helper()
        {
            var assembly = File.ReadAllBytes(@"C:\Temp\Version1\ILoggingDummyImp.dll");
            ModuleRegistrationFactory.Initialize(@"C:\Temp\Modules\Test");
            ModuleRegistrationFactory.ReEnumerate();
            if (Directory.GetFiles(@"C:\Temp\Modules\Test").Length != 0) return;
            ModuleRegistrationFactory.ImportAssemblyWith<ILogging>("Test", assembly);
        }
        [TestMethod]
        [TestCategory("Module registration")]
        public void TestIComponentRegistrationSelector()
        {
            if(!ModuleRegistrationFactory.UsesDefaultComponentRegistrationModule())
                ModuleRegistrationFactory.SetDefaultComponentRegistrationModule();
            ModuleRegistrationFactory.ResetHandlerTo<DummyComponentRegistration>();
            Assert.IsFalse(ModuleRegistrationFactory.UsesDefaultComponentRegistrationModule());
            ModuleRegistrationFactory.SetDefaultComponentRegistrationModule();
        }

        /// <summary>
        ///A test for CreateNewComponentRegistration
        ///</summary>
        [TestMethod()]
        [TestCategory("Module registration")]
        public void CreateNewComponentRegistrationTest()
        {
            var path = "asdfasdf";
            var expected = "Stardust.Core.CrossCuttingTest.LegacyTests.DummyComponentRegistration";
            ModuleRegistrationFactory.ResetHandlerTo<DummyComponentRegistration>();
            var actual = ModuleRegistrationFactory.CreateNewComponentRegistration(path);
            Assert.IsInstanceOfType(actual,typeof(DummyComponentRegistration));
            Assert.AreEqual(expected,ModuleRegistrationFactory.GetComponentRegistrationName());
            Assert.IsFalse(ModuleRegistrationFactory.UsesDefaultComponentRegistrationModule());
            ModuleRegistrationFactory.SetDefaultComponentRegistrationModule();
        }

        [TestMethod()]
        [TestCategory("Module registration")]
        public void GetImpelmetationsOf()
        {
            CleanTestFolder();
            var assembly = File.ReadAllBytes(@"C:\Temp\Version1\ILoggingDummyImp.dll");
            ModuleRegistrationFactory.Initialize(@"C:\Temp\Modules");
            ModuleRegistrationFactory.ReEnumerate();
            ModuleRegistrationFactory.ImportAssemblyWith<ILogging>("Test", assembly);
            var actual = ModuleRegistrationFactory.GetAllRegisteredImplementationsFor<ILogging>();
            Assert.AreEqual(1,actual.Length);
        }
    }
}
