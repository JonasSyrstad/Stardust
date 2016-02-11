//
// ArrayExtentionTests.cs
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
using Stardust.Particles.Collection.Arrays;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    [TestClass]
    public class ArrayExtensionTests
    {
        /// <summary>
        ///A test for ContainsElements
        ///</summary>
        [TestMethod()]
        [TestCategory("Array extensions")]
        public void ContainsElements_Null_Test()
        {
            int[] self = null;
            bool expected = false; 
            bool actual;
            actual = ArrayExtensions.ContainsElements(self);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ContainsElements
        ///</summary>
        [TestMethod()]
        [TestCategory("Array extensions")]
        public void ContainsElements_Empty_Test()
        {
            var self = new int[0]; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = ArrayExtensions.ContainsElements(self);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ContainsElements
        ///</summary>
        [TestMethod()]
        [TestCategory("Array extensions")]
        public void ContainsElements_WithElements_Test()
        {
            var self = new int[] { 1, 2, 3, 4 };
            bool expected = true; // TODO: Initialize to an appropriate value
            bool actual;
            actual = ArrayExtensions.ContainsElements(self);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for IsEmpty
        ///</summary>
        [TestMethod()]
        [TestCategory("Array extensions")]
        public void IsEmpty_Null_Test()
        {
            int[] self = null; // TODO: Initialize to an appropriate value
            bool expected = true; // TODO: Initialize to an appropriate value
            bool actual;
            actual = ArrayExtensions.IsEmpty(self);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for IsEmpty
        ///</summary>
        [TestMethod()]
        [TestCategory("Array extensions")]
        public void IsEmpty_Empty_Test()
        {
            var self = new int[0]; // TODO: Initialize to an appropriate value
            bool expected = true; // TODO: Initialize to an appropriate value
            bool actual;
            actual = ArrayExtensions.IsEmpty(self);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for IsEmpty
        ///</summary>
        [TestMethod()]
        [TestCategory("Array extensions")]
        public void IsEmpty_WithElements_Test()
        {
            var self = new int[] { 1, 2, 3, 4 };
            bool expected = false;
            bool actual;
            actual = ArrayExtensions.IsEmpty(self);
            Assert.AreEqual(expected, actual);

        }
    }
}
