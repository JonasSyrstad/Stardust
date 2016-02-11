//
// IEnumerableExtentionsTest.cs
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Core.CrossCuttingTest.LegacyTests.Mock;
using Stardust.Particles.Collection;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    
    
    /// <summary>
    ///This is a test class for IEnumerableExtentionsTest and is intended
    ///to contain all IEnumerableExtentionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IEnumerableExtentionsTest
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
        ///A test for ContainsElements
        ///</summary>
        [TestMethod()]
        [TestCategory("Enum helper")]
        public void ContainsElements_Null_Test()
        {

            IList<object> self = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = self.ContainsElements();
            Assert.AreEqual(expected, actual);
            
        }

        /// <summary>
        ///A test for ContainsElements
        ///</summary>
        [TestMethod()]
        [TestCategory("Enum helper")]
        public void ContainsElements_Empty_Test()
        {
            IList<object> self = new List<object>(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = self.ContainsElements();
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ContainsElements
        ///</summary>
        [TestMethod()]
        [TestCategory("Enum helper")]
        public void ContainsElements_WithElements_Test()
        {
            List<object> self = new List<object>();
            self.Add(new object());
            bool expected = true; // TODO: Initialize to an appropriate value
            bool actual;
            actual = EnumerableExtensions.ContainsElements(self);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for IsEmpty
        ///</summary>
        [TestMethod()]
        [TestCategory("Enum helper")]
        public void IsEmpty_Null_Test()
        {
            IList<object> self = null; // TODO: Initialize to an appropriate value
            bool expected = true; // TODO: Initialize to an appropriate value
            bool actual;
            actual = self.IsEmpty();
            Assert.AreEqual(expected, actual);
            
        }

        /// <summary>
        ///A test for IsEmpty
        ///</summary>
        [TestMethod()]
        [TestCategory("Enum helper")]
        public void IsEmpty_Empty_Test()
        {
            IList<object> self = new List<object>(); // TODO: Initialize to an appropriate value
            bool expected = true; // TODO: Initialize to an appropriate value
            bool actual;
            actual = self.IsEmpty();
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for IsEmpty
        ///</summary>
        [TestMethod()]
        [TestCategory("Enum helper")]
        public void IsEmpty_WithElements_Test()
        {
            List<object> self = new List<object>();
            self.Add(new object());
            bool expected = false; 
            bool actual;
            actual = EnumerableExtensions.IsEmpty(self);
            
            var q = self.AsQueryable().Where(s => ((StringClass) s).Header3 == "test");
            Assert.AreEqual(expected, actual);
        }
    }
}
