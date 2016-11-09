//
// EnumExtentionsTest.cs
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
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Particles;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{


    /// <summary>
    ///This is a test class for EnumExtentionsTest and is intended
    ///to contain all EnumExtentionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EnumExtentionsTest
    {
        enum test { test1 = 0, test2, test3, test4,test5,test6,test7,test8 }
        [TestMethod]
        public void EnumToStringTest()
        {
            var rnd = new Random();
            var warmupString = test.test1.FastToString();
            var timer = Stopwatch.StartNew();
            nomberOfItterrations = 10000000;
            for (int i = 0; i < nomberOfItterrations; i++)
            {
                var s = ((test)rnd.Next(0, 7)).FastToString();
            }
            timer.Stop();
            var fastTOstring = timer.ElapsedTicks;
            timer.Reset();
            timer.Start();
            for (int i = 0; i < nomberOfItterrations; i++)
            {
                var e = (test)rnd.Next(0, 7);
                var s = e.ToString();
            }
            timer.Stop();
            var toString = timer.ElapsedTicks;
            Assert.IsTrue(fastTOstring<toString);
            Console.WriteLine($"fast: {fastTOstring} < {toString}");
            var dif = (decimal)fastTOstring / toString;
            Console.WriteLine($"dif: {dif.ToString("N8")}");
        }


        private TestContext testContextInstance;

        private int nomberOfItterrations;

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
        public void EnumToListTest()
        {
            var expected = new List<MyEnum>() { MyEnum.Me, MyEnum.You, MyEnum.They }; // TODO: Initialize to an appropriate value
            var actual = EnumHelper.EnumToList<MyEnum>();
            Assert.AreEqual(expected[0], actual[0]);
            Assert.AreEqual(expected[1], actual[1]);
            Assert.AreEqual(expected[2], actual[2]);
        }
    }
    public enum MyEnum
    {
        Me,
        You,
        They
    }
}
