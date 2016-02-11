//
// DelimiteredFileReaderTest.cs
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Clusters;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;
using Stardust.Particles.TableParser;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{


    /// <summary>
    ///This is a test class for DelimiteredFileReaderTest and is intended
    ///to contain all DelimiteredFileReaderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DelimiteredFileReaderTest
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


        /// <summary>
        ///A test for ReadDocument
        ///</summary>
        [TestMethod()]
        [TestCategory("Tabular file parser")]
        [DeploymentItem("TestFiles/DelimiteredTestFile.txt")]
        public void ReadDocumentTest()
        {
            FrameworkInitializer.InitializeModules();
            var target = Parsers.Delimitered.GetParser().Initialize("|", "\n", "\"", true);
            var fileName = @"DelimiteredTestFile.txt";
            Document actual;
            actual = target.Parse(fileName);
            Assert.AreEqual(2, actual.NumberOfRows);
            Assert.IsTrue(actual.HaveHeader);
            Assert.IsTrue(actual.IsValid());
        }

        [TestMethod]
        [TestCategory("Tabular file parser")]
        [DeploymentItem("TestFiles/XMLTestFile.xml")]
        public void ParseXmlDocumentTest()
        {
            
            var parser = Parsers.SimpleXmlParser.GetParser();
            parser.SetProperty("RootName", "root");
            var actual = parser.Parse(@"XMLTestFile.xml");
            Assert.IsTrue(actual.HaveHeader);
            Assert.AreEqual(2, actual.NumberOfRows);
            Assert.AreEqual(3, actual.Header.Count());
            Assert.IsTrue(actual.IsValid());
            actual.Validate();
        }

        [TestMethod]
        [TestCategory("Tabular file parser")]
        public void SetPropertyTest()
        {
            var parser = Parsers.SimpleXmlParser.GetParser();
            parser.SetProperty("RootName", "root");
            Assert.IsTrue(parser.HasExtendedProperties());
        }

        [TestMethod]
        [TestCategory("Tabular file parser")]
        public void SetPropertyIncorrectNameTest()
        {
            var parser = Parsers.SimpleXmlParser.GetParser();
            try
            {
                parser.SetProperty("RootName121", "root");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(StardustCoreException));
            }
        }

        [TestMethod]
        [TestCategory("Tabular file parser")]
        public void GetDynamicPropertiesTest()
        {
            var parser = Parsers.SimpleXmlParser.GetParser();
            var param = parser.GetConfigurableProperties();
            Assert.AreEqual(1, param.Length);
        }

        [TestMethod]
        [TestCategory("Tabular file parser")]
        public void SetPropertyIncorrecttypeTest()
        {
            var parser = Parsers.SimpleXmlParser.GetParser();
            try
            {
                parser.SetProperty("RootName", 12);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(StardustCoreException));
            }
        }

        [TestMethod]
        [TestCategory("Tabular file parser")]
        [DeploymentItem("TestFiles/OldExcelTestFile.xls")]
        public void ParseOldExcelTest()
        {
            var parser = ParserFactory.GetParser(Parsers.OldExcel).SetHeaderRow(true);
            var actual = parser.Parse(@"OldExcelTestFile.xls");
            Assert.IsTrue(actual.HaveHeader);
            Assert.AreEqual(2, actual.NumberOfRows);
            Assert.AreEqual(3, actual.Header.Count());
            Assert.IsTrue(actual.IsValid());
        }

        [TestMethod]
        [TestCategory("Tabular file parser")]
        [DeploymentItem("TestFiles/NewExcelTestFile.xlsx")]
        public void ParseNewExcelTest()
        {
            var parser = ParserFactory.GetParser(Parsers.Excel).SetHeaderRow(true);
            var actual = parser.Parse(@"NewExcelTestFile.xlsx");
            Assert.IsTrue(actual.HaveHeader);
            Assert.AreEqual(2, actual.NumberOfRows);
            Assert.AreEqual(3, actual.Header.Count());
            Assert.IsTrue(actual.IsValid());
        }
    }
}
