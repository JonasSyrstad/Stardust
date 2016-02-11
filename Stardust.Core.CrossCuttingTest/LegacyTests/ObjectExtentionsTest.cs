//
// ObjectExtentionsTest.cs
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
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Particles;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    
    
    /// <summary>
    ///This is a test class for ObjectExtentionsTest and is intended
    ///to contain all ObjectExtentionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ObjectExtentionsTest
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
        ///A test for Cast
        ///</summary>
        [TestMethod()]
        [TestCategory("Casting")]
        public void CastTest()
        {
            object self = 100; // TODO: Initialize to an appropriate value
            var expected = 100; // TODO: Initialize to an appropriate value
            int actual;
            actual = self.Cast<int>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [TestCategory("Casting")]
        public void CastTestFails()
        {
            object self = 100; // TODO: Initialize to an appropriate value
            var expected = 100; // TODO: Initialize to an appropriate value
            long actual;
            try
            {
                actual = self.Cast<long>();
            }
            catch(Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof (StardustCoreException));
            }

        }

        /// <summary>
        ///A test for TryDispose
        ///</summary>
        [TestMethod()]
        [TestCategory("Casting")]
        public void TryDisposeTest()
        {
            object self = new MemoryStream(); // TODO: Initialize to an appropriate value
            self.TryDispose();
            try
            {
                var c=((MemoryStream) self).Capacity;
                Assert.Fail();
            }
            catch(Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof (ObjectDisposedException));
            }
        }

        [TestMethod()]
        [TestCategory("Casting")]
        public void CloneCloneableObjectTest()
        {
            object clonable = new Cloneable {Name = "Jonas Syrstad", Address = "Setesdalsveien 68"};
            var clone = clonable.Clone();
            Assert.AreEqual(((Cloneable)clonable).Name,((Cloneable)clone).Name);
            Assert.AreEqual(((Cloneable)clonable).Address, ((Cloneable)clone).Address);
            Assert.AreNotSame(clonable,clone);
        }

        [TestMethod()]
        [TestCategory("Casting")]
        public void CloneNonCloneableObjectTest()
        {
            var clonable = new object();
            var clone = clonable.Clone();
            Assert.IsNull(clone);
        }
    }
    public class Cloneable : ICloneable
    {
        public String Name { get; set; }
        public string Address { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }   
}
