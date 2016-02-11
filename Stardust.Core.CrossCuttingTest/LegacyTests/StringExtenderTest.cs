//
// StringExtenderTest.cs
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
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Core.CrossCuttingTest.LegacyTests.Mock;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;
using Stardust.Particles.Xml;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{


    /// <summary>
    ///This is a test class for StringExctenderTest and is intended
    ///to contain all StringExctenderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StringExtenderTest
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
        ///A test for WriteToFile
        ///</summary>
        [TestMethod()]
        [TestCategory("String extensions")]
        public void WriteToFileUnicodeTest()
        {
            string self = "testFile"; // TODO: Initialize to an appropriate value
            string filePath = @"C:\temp\testfile.txt"; // TODO: Initialize to an appropriate value
            if (File.Exists(filePath))
                File.Delete(filePath);
            EncodingType encoding = EncodingType.Unicode; // TODO: Initialize to an appropriate value
            self.WriteToFile(filePath, encoding);
            Assert.IsTrue(File.Exists(filePath));
        }

        [TestMethod()]
        [TestCategory("String extensions")]
        public void WriteToFileAsciiTest()
        {
            string self = "testFile"; // TODO: Initialize to an appropriate value
            string filePath = @"C:\temp\testfile.txt"; // TODO: Initialize to an appropriate value
            if (File.Exists(filePath))
                File.Delete(filePath);
            EncodingType encoding = EncodingType.Ascii; // TODO: Initialize to an appropriate value
            self.WriteToFile(filePath, encoding);
            Assert.IsTrue(File.Exists(filePath));
        }

        /// <summary>
        ///A test for Transform
        ///</summary>
        [TestMethod()]
        [TestCategory("String extensions")]
        public void GetMatchesTest()
        {
            string self = "Who writes these notes?"; // TODO: Initialize to an appropriate value
            string pattern = @"\b\w+es\b";
            int expected = 2; // TODO: Initialize to an appropriate value
            MatchCollection actual;
            actual = self.GetMatches(pattern);
            Assert.AreEqual(expected, actual.Count);

        }

        /// <summary>
        ///A test for IsValid
        ///</summary>
        [TestMethod()]
        [TestCategory("String extensions")]
        public void IsValid_True_Test()
        {
            string self = "1298-673-4192"; // TODO: Initialize to an appropriate value
            string pattern = @"^[a-zA-Z0-9]\d{2}[a-zA-Z0-9](-\d{3}){2}[A-Za-z0-9]$"; // TODO: Initialize to an appropriate value
            bool expected = true; // TODO: Initialize to an appropriate value
            bool actual;
            actual = self.IsValid(pattern);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for IsValid
        ///</summary>
        [TestMethod()]
        [TestCategory("String extensions")]
        public void IsValid_False_Test()
        {
            string self = "_A90-123-129X"; // TODO: Initialize to an appropriate value
            string pattern = @"^[a-zA-Z0-9]\d{2}[a-zA-Z0-9](-\d{3}){2}[A-Za-z0-9]$"; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = self.IsValid(pattern);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("String extensions")]
        public void SerializableObjectIsSerializable()
        {
            string self = "dette er en test";
            bool actual = self.IsSerializable();
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [TestCategory("String extensions")]
        public void NonSerializableObjectIsSerializable()
        {
            var self = new NonSerializableObject();
            bool actual = self.IsSerializable();
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [TestCategory("String extensions")]
        public void SerializeSafeObject()
        {
            var self = "test";
            var actual = self.Serialize(includeTypeInNamespace: true);
            Assert.IsTrue(actual.Contains("test"));
            Assert.IsTrue(actual.Contains(actual.GetType().FullName));
        }

        [TestMethod]
        [TestCategory("String extensions")]
        public void SerializeUnsafeObjectSuccess()
        {
            var self = new NonSerializableObject();
            self.AString = "test";
            self.AnInt = 12;
            var actual = self.Serialize("http://unsafe.com/", true);
            Assert.IsTrue(actual.Contains("test"));
            Assert.IsTrue(actual.Contains("http://unsafe.com/"));
        }

        [TestMethod]
        [TestCategory("String extensions")]
        public void SerializeUnsafeObjectFails()
        {
            var self = new NonSerializableObject();
            self.AString = "test";
            self.AnInt = 12;
            string actual = null;
            try
            {
                actual = self.Serialize("http://unsafe.com/");
                Assert.Fail();
            }
            catch
            {
                Assert.AreEqual(null, actual);
            }
        }

        string filePath = @"c:\temp\unit_test.xml";

        [TestMethod]
        [TestCategory("String extensions")]
        public void SerializeSafeObjectToFile()
        {

            var self = new NonSerializableObject();
            self.AString = "test";
            self.AnInt = 12;
            if (File.Exists(filePath))
                File.Delete(filePath);
            self.SerializeToFile(filePath, true);
            Assert.IsTrue(File.Exists(filePath));
        }

        [TestMethod]
        [TestCategory("String extensions")]
        public void StringStartsWithOneOf_True_Test()
        {
            var self = "System.String";
            var actual = self.StartsWithOneOfCaseInsensitive("system", "test", "foo");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [TestCategory("String extensions")]
        public void StringStartsWithOneOf_False_Test()
        {
            var self = "System.String";
            var actual = self.StartsWithOneOfCaseInsensitive("test", "foo", "bar");
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [TestCategory("String extensions")]
        public void StringStartsWithOneOfCaseSensitive_True_Test()
        {
            var self = "System.String";
            var actual = self.StartsWithOneOf("System", "test", "foo");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [TestCategory("String extensions")]
        public void StringStartsWithOneOfCaseSensitive_False_Test()
        {
            var self = "System.String";
            var actual = self.StartsWithOneOf("system", "test", "foo");
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [TestCategory("String extensions")]
        public void StringContanisOneOfCaseSensitive_True_Test()
        {
            var self = "System.String";
            var actual = self.ContainsOneOf("System", "test", "foo");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [TestCategory("String extensions")]
        public void StringContanisOneOfCaseSensitive_False_Test()
        {
            var self = "System.String";
            var actual = self.ContainsOneOf("system", "test", "foo");
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [TestCategory("String extensions")]
        public void StringEqualsOneOfCaseSensitive_True_Test()
        {
            var self = "System";
            var actual = self.EqualsOneOf("System", "test", "foo");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [TestCategory("String extensions")]
        public void StringEqualsOneOfCaseSensitive_False_Test()
        {
            var self = "System";
            var actual = self.EqualsOneOf("system", "test", "foo");
            Assert.IsFalse(actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        [TestCategory("String extensions")]
        public void ToStringWithCustomSeparatorTest()
        {
            List<string> list = new List<string>();
            list.Add("1");
            list.Add("2");
            list.Add("3");
            list.Add("4");
            string separator = "|"; // TODO: Initialize to an appropriate value
            string expected = "1|2|3|4"; // TODO: Initialize to an appropriate value
            string actual;
            actual = list.ToSeparatedString(separator);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        [TestCategory("String extensions")]
        public void ToStringWithDefaultSeparatorTest()
        {
            List<string> list = new List<string>();
            list.Add("1");
            list.Add("2");
            list.Add("3");
            list.Add("4");

            string expected = "1;2;3;4";
            string actual;
            actual = list.ToSeparatedString();
            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void SplitStringWithToken()
        {
            var spliter = "split";
            var content = "JonassplitSyrstad";
            var lines = content.Split(new string[] { spliter }, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2,lines.Length);
            Assert.AreEqual("Jonas",lines[0]);
            Assert.AreEqual("Syrstad", lines[1]);

        }
    }
}
