//
// LoggingTest.cs
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
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Core.CrossCuttingTest.LegacyTests.Mock;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    
    
    /// <summary>
    ///This is a test class for LoggingTest and is intended
    ///to contain all LoggingTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LoggingTest
    {
        private IKernelContext KernelScope;

        [TestInitialize]
        public void Initialize()
        {
            ContainerFactory.Current.KillAllInstances();
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

        string filePath=@"C:\temp\logging\log.txt";
        /// <summary>
        ///A test for DebugMessage
        ///</summary>
        [TestMethod()]
        [TestCategory("Logging")]
        public void DebugMessageTest()
        {
            
            Logging.ResetLogger<LoggingImplementation>();
            ContainerFactory.Current.KillAllInstances();
            File.Delete(filePath);
            var testStart = File.GetLastWriteTime(filePath);
            string message = "test"; // TODO: Initialize to an appropriate value
            EventLogEntryType entryType = EventLogEntryType.Information; // TODO: Initialize to an appropriate value
            string additionalDebugInformation = "run"; // TODO: Initialize to an appropriate value
            Logging.DebugMessage(message, entryType, additionalDebugInformation);
            Assert.IsTrue(File.Exists(filePath));
            Assert.AreEqual(1, File.ReadAllLines(filePath).Length); 
        }

        [TestMethod]
        [TestCategory("Logging")]
        public void InsertHeartBeatTest()
        {
            Logging.ResetLogger<LoggingImplementation>();
            File.Delete(filePath);
            Logging.HeartBeat();
            Assert.IsTrue(File.Exists(filePath));
            Assert.AreEqual(1, File.ReadAllLines(filePath).Length); 
        }

        /// <summary>
        ///A test for DebugMessage
        ///</summary>
        [TestMethod()]
        [TestCategory("Logging")]
        public void ExceptionMessageTest()
        {
            File.Delete(filePath);
            Logging.ResetLogger<LoggingImplementation>();
            string t = null;
            try
            {
                t.Contains("test");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                var outer = new Exception("UnexpectedError", ex);
                string additionalDebugInformation = "run";
                int expected = 4;
                Logging.Exception(outer, additionalDebugInformation);
                Assert.IsTrue(File.Exists(filePath));
                Assert.AreEqual(expected, File.ReadAllLines(filePath).Length);
            }
        }

        /// <summary>
        ///A test for ResetLogger
        ///</summary>
        [TestMethod()]
        [TestCategory("Logging")]
        public void ResetLoggerTest()
        {
            Action<ILogging> initializationHandler = null; // TODO: Initialize to an appropriate value
            Logging.ResetLogger();
            Assert.IsTrue(Logging.Initialized());
        }


        /// <summary>
        ///A test for InitializeModuleCreatorWithDefalutLogger
        ///</summary>
        [TestMethod()]
        [TestCategory("Logging")]
        public void InitializeModuleCreatorWithDefalutLoggerTest()
        {
            Resolver.GetConfigurator().UnBind<ILogging>().All();
            Logging.InitializeModuleCreatorWithDefalutLogger();
            Assert.IsTrue(Logging.Initialized());
            Resolver.GetConfigurator().UnBind<ILogging>().AllAndBind().To<Mock.LoggingImplementation>();
        }
    }
}
