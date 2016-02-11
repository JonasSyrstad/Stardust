//
// ExsistanceExtenetionsTest.cs
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
using Stardust.Particles;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    
    
    /// <summary>
    ///This is a test class for ExsistanceExtenetionsTest and is intended
    ///to contain all ExsistanceExtenetionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ExsistanceExtenetionsTest
    {


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
        ///A test for IsNullOrEmpty
        ///</summary>
        [TestMethod()]
        [TestCategory("Instance existence")]
        public void IsNullOrEmpty_EmptyString_Test()
        {
            string self = string.Empty; 
            bool expected = true; 
            bool actual;
            actual = self.IsNullOrEmpty();
            Assert.AreEqual(expected, actual);
            
        }

        /// <summary>
        ///A test for IsNullOrEmpty
        ///</summary>
        [TestMethod()]
        [TestCategory("Instance existence")]
        public void ContainsCharacters_WithCharacters_Test()
        {
            string self = "testdata"; 
            bool expected = true;
            bool actual;
            actual = self.ContainsCharacters();
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for IsNullOrEmpty
        ///</summary>
        [TestMethod()]
        [TestCategory("Instance existence")]
        public void IsNullOrEmpty_Null_Test()
        {
            string self = null;
            bool expected = true; 
            bool actual;
            actual = self.IsNullOrEmpty();
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for IsNullOrEmpty
        ///</summary>
        [TestMethod()]
        [TestCategory("Instance existence")]
        public void IsNullOrEmpty_WithData_Test()
        {
            string self = "testData";
            bool expected = false;
            bool actual;
            actual = self.IsNullOrEmpty();
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for IsNull
        ///</summary>
        [TestMethod()]
        [TestCategory("Instance existence")]
        public void IsNull_Null_Test()
        {
            object self = null;
            bool expected = true;
            bool actual;
            actual = self.IsNull();
            Assert.AreEqual(expected, actual);
            
        }

        /// <summary>
        ///A test for IsNull
        ///</summary>
        [TestMethod()]
        [TestCategory("Instance existence")]
        public void IsNull_WithData_Test()
        {
            object self = "testdata";
            bool expected = false;
            bool actual;
            actual = self.IsNull();
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for IsInstance
        ///</summary>
        [TestMethod()]
        [TestCategory("Instance existence")]
        public void IsInstance_Null_Test()
        {
            object self = null;
            bool expected = false;
            bool actual;
            actual = self.IsInstance();
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsInstance
        ///</summary>
        [TestMethod()]
        [TestCategory("Instance existence")]
        public void IsInstance_WithData_Test()
        {
            object self = "testdata";
            bool expected = true;
            bool actual;
            actual = self.IsInstance();
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
