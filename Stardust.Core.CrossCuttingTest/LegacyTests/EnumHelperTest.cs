//
// EnumHelperTest.cs
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
using Stardust.Particles;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{


    /// <summary>
    ///This is a test class for EnumHelperTest and is intended
    ///to contain all EnumHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EnumHelperTest
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


        [TestMethod()]
        [TestCategory("Enum helper")]
        public void ToIntArrayTest()
        {
            var enums = new[] { TestEnum.Fail, TestEnum.Success };
            var ints = enums.ToIntArray();
            Assert.AreEqual(2, ints.Length);
            Assert.AreEqual(1, ints[0]);
            Assert.AreEqual(2, ints[1]);
        }


        [TestMethod()]
        [TestCategory("Enum helper")]
        public void ToEnumArrayTest()
        {
            var ints = new[] { 1, 2 };
            var enums = ints.ToEnumArray<TestEnum>();
            Assert.AreEqual(2, enums.Length);
            Assert.AreEqual(TestEnum.Fail, enums[0]);
            Assert.AreEqual(TestEnum.Success, enums[1]);
        }

        [TestMethod()]
        [TestCategory("Enum helper")]
        public void ParseAsEnumTest()
        {
            var test = "Test";
            var enums = test.ParseAsEnum<TestEnum>();
            Assert.AreEqual(TestEnum.Test, enums);
        }

        [TestMethod()]
        [TestCategory("Enum helper")]
        public void ParseAsEnumFailTest()
        {
            var test = "Testasdfsad";
            try
            {
                var enums = test.ParseAsEnum<TestEnum>();
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(StardustCoreException));
            }

        }

        [TestMethod()]
        [TestCategory("Enum helper")]
        public void ParseAsEnumWithDefaultTest()
        {
            var test = "Test";
            var enums = test.ParseAsEnum(TestEnum.Success);
            Assert.AreEqual((object) TestEnum.Test, enums);
        }

        [TestMethod()]
        [TestCategory("Enum helper")]
        public void ParseAsEnumFailWithDefaultTest()
        {
            var test = "Testasdfsad";
            try
            {
                var enums = test.ParseAsEnum(TestEnum.Success);
                Assert.AreEqual((object) TestEnum.Success, enums);
            }
            catch (Exception)
            {
                Assert.Fail();
            }

        }
    }
    public enum TestEnum
    {
        Test,
        Fail,
        Success
    }
}
