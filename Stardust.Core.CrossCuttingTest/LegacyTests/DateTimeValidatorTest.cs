//
// DateTimeValidatorTest.cs
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Particles.Validator;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{


    /// <summary>
    ///This is a test class for DateTimeValidatorTest and is intended
    ///to contain all DateTimeValidatorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DateTimeValidatorTest
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

        #region Greater than tests

        /// <summary>
        ///A test for IsGreaterOrEqualThan
        ///</summary>
        [TestMethod()]
        [TestCategory("Date time validator")]
        public void IsGreaterOrEqualThan_Equal_Test()
        {
            DateTime self = new DateTime(2011, 5, 5);
            var actual = self.IsGreaterOrEqualThan(self);
            Assert.IsTrue(actual);


        }

        [TestMethod()]
        [TestCategory("Date time validator")]
        public void IsGreaterOrEqualThan_OneSecLarger_Test()
        {
            DateTime self = new DateTime(2011, 5, 5);
            var actual = self.IsGreaterOrEqualThan(self.AddSeconds(1));
            Assert.IsFalse(actual);


        }

        [TestMethod()]
        [TestCategory("Date time validator")]
        public void IsGreaterOrEqualThan_OneSecLess_Test()
        {
            DateTime self = new DateTime(2011, 5, 5);
            var actual = self.IsGreaterOrEqualThan(self.AddSeconds(-1));
            Assert.IsTrue(actual);


        }

        [TestMethod()]
        [TestCategory("Date time validator")]
        public void IsGreater_Equal_Test()
        {
            DateTime self = new DateTime(2011, 5, 5);
            var actual = self.IsGreaterThan(self);
            Assert.IsFalse(actual);


        }

        [TestMethod()]
        [TestCategory("Date time validator")]
        public void IsGreater_OneSecLarger_Test()
        {
            DateTime self = new DateTime(2011, 5, 5);
            var actual = self.IsGreaterThan(self.AddSeconds(1));
            Assert.IsFalse(actual);


        }

        [TestMethod()]
        [TestCategory("Date time validator")]
        public void IsGreater_OneSecLess_Test()
        {
            DateTime self = new DateTime(2011, 5, 5);
            var actual = self.IsGreaterThan(self.AddSeconds(-1));
            Assert.IsTrue(actual);


        }
        #endregion

        #region Less than tests
        [TestMethod()]
        [TestCategory("Date time validator")]
        public void IsLessOrEqualThan_Equal_Test()
        {
            DateTime self = new DateTime(2011, 5, 5);
            var actual = self.IsLessOrEqualThan(self);
            Assert.IsTrue(actual);


        }

        [TestMethod()]
        [TestCategory("Date time validator")]
        public void IsLessOrEqualThan_OneSecLarger_Test()
        {
            DateTime self = new DateTime(2011, 5, 5);
            var actual = self.IsLessOrEqualThan(self.AddSeconds(1));
            Assert.IsTrue(actual);


        }

        [TestMethod()]
        [TestCategory("Date time validator")]
        public void IsLessOrEqualThan_OneSecLess_Test()
        {
            DateTime self = new DateTime(2011, 5, 5);
            var actual = self.IsLessOrEqualThan(self.AddSeconds(-1));
            Assert.IsFalse(actual);


        }

        [TestMethod()]
        [TestCategory("Date time validator")]
        public void IsLess_Equal_Test()
        {
            DateTime self = new DateTime(2011, 5, 5);
            var actual = self.IsLessThan(self);
            Assert.IsFalse(actual);


        }

        [TestMethod()]
        [TestCategory("Date time validator")]
        public void IsLess_OneSecLarger_Test()
        {
            DateTime self = new DateTime(2011, 5, 5);
            var actual = self.IsLessThan(self.AddSeconds(1));
            Assert.IsTrue(actual);


        }

        [TestMethod()]
        [TestCategory("Date time validator")]
        public void IsLess_OneSecLess_Test()
        {
            DateTime self = new DateTime(2011, 5, 5);
            var actual = self.IsLessThan(self.AddSeconds(-1));
            Assert.IsFalse(actual);


        }
        #endregion

        #region Range tests
        [TestMethod]
        [TestCategory("Date time validator")]
        public void InRange_True_Test()
        {
            DateTime self = new DateTime(2011, 2, 2);
            DateTime start = self.AddSeconds(-1);
            DateTime end = self.AddSeconds(1);

            Assert.IsTrue(self.IsInRange(start, end));
        }

        [TestMethod]
        [TestCategory("Date time validator")]
        public void InRange_ExactBorderLow_Test()
        {
            DateTime self = new DateTime(2011, 2, 2);
            DateTime start = self;
            DateTime end = self.AddSeconds(1);

            Assert.IsFalse(self.IsInRange(start, end));
        }

        [TestMethod]
        [TestCategory("Date time validator")]
        public void InRange_ExactBorderHigh_Test()
        {
            DateTime self = new DateTime(2011, 2, 2);
            DateTime start = self.AddSeconds(-1);
            DateTime end = self;

            Assert.IsFalse(self.IsInRange(start, end));
        }

        [TestMethod]
        [TestCategory("Date time validator")]
        public void InRangeExact_True_Test()
        {
            DateTime self = new DateTime(2011, 2, 2);
            DateTime start = self.AddSeconds(-1);
            DateTime end = self.AddSeconds(1);

            Assert.IsTrue(self.IsInRangeIncludingEdges(start, end));
        }

        [TestMethod]
        [TestCategory("Date time validator")]
        public void InRangeExcact_ExactBorderLow_Test()
        {
            DateTime self = new DateTime(2011, 2, 2);
            DateTime start = self;
            DateTime end = self.AddSeconds(1);

            Assert.IsTrue(self.IsInRangeIncludingEdges(start, end));
        }

        [TestMethod]
        [TestCategory("Date time validator")]
        public void InRangeExact_ExactBorderHigh_Test()
        {
            DateTime self = new DateTime(2011, 2, 2);
            DateTime start = self.AddSeconds(-1);
            DateTime end = self;

            Assert.IsTrue(self.IsInRangeIncludingEdges(start, end));
        }

        [TestMethod]
        [TestCategory("Date time validator")]
        public void InRange_outsideExactBorderLow_Test()
        {
            DateTime self = new DateTime(2011, 2, 2);
            DateTime start = self.AddSeconds(1);
            DateTime end = self.AddSeconds(2);

            Assert.IsFalse(self.IsInRangeIncludingEdges(start, end));
        }

        [TestMethod]
        [TestCategory("Date time validator")]
        public void InRange_outsideBorderHigh_Test()
        {
            DateTime self = new DateTime(2011, 2, 2);
            DateTime start = self.AddSeconds(-2);
            DateTime end = self.AddSeconds(-1);

            Assert.IsFalse(self.IsInRangeIncludingEdges(start, end));
        }


        #endregion
    }
}
