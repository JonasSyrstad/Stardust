//
// TransferFactoryTest.cs
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

using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Clusters;
using Stardust.Core.Security;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;
using Stardust.Particles.FileTransfer;
using Stardust.Particles.Collection.Arrays;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    /// <summary>
    ///This is a test class for TransferFactoryTest and is intended
    ///to contain all TransferFactoryTest Unit Tests
    ///</summary>
    [TestClass]
    public class TransferFactoryTest
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
        private string FileName;
        private string alternateFileName;

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

        #endregion Additional test attributes

        /// <summary>
        ///A test for Create
        ///</summary>
        [TestMethod()]
        [TestCategory("File transfer")]
        public void CreateFtpReadTest()
        {
            var type = TransferMethods.Ftp;
            var expected = typeof(FtpTrasfer);
            var actual = TransferFactory.Create(type)
                .SetServerRootUrl("127.0.0.1")
                .SetFileName("/test/test.txt").SetUserNameAndPassword(GetUserName(), GetPassword());
            var file = actual.Read();
            var encoding = EncodingFactory.Check(file, file.Length);
            var data = encoding.GetString(file);
            Assert.IsInstanceOfType(actual, expected);
            Assert.IsTrue("Dette er en test".Equals(data));
        }

        [TestMethod()]
        [TestCategory("File transfer")]
        public void CreateFileReadTest()
        {
            var type = TransferMethods.File;
            var expected = typeof(FileTransfer);
            var actual = TransferFactory.Create(type)
                .SetServerRootUrl(@"C:\inetpub\ftproot\Test\")
                .SetFileName("test.txt");
            //.SetUserNameAndPassword(GetUserName(), GetPassword());
            var file = actual.Read();
            var encoding = EncodingFactory.Check(file, file.Length);
            var data = encoding.GetString(file);
            Assert.IsInstanceOfType(actual, expected);
            Assert.IsTrue("Dette er en test".Equals(data));
        }

        //[TestMethod()]
        public void CreateHttpReadTest()
        {
            var type = TransferMethods.Http;
            var expected = typeof(HttpFileTrasfer);
            var actual = TransferFactory.Create(type)
                .SetServerRootUrl(@"http://pdv-devwintfs1")
                .SetFileName("/Shared%20Documents/Test.txt")
                .SetUserNameAndPassword(GetHttpUserName(), GetPassword());
            var file = actual.Read();
            var data = EncodingFactory.Check(file, file.Length).GetString(file);
            Assert.IsInstanceOfType(actual, expected);
            Assert.AreEqual("Dette er en test", data);
        }

        //http://pdv-devwintfs1/Shared%20Documents/Test.txt
        /// <summary>
        ///A test for Create
        ///</summary>
        [TestMethod()]
        [TestCategory("File transfer")]
        public void CreateFolder()
        {
            if (Directory.Exists(@"C:\inetpub\ftproot\UnitTest"))
                Directory.Delete(@"C:\inetpub\ftproot\UnitTest");
            var client = GetFtpClient();
            client.Mkdir("UnitTest");
            Assert.IsTrue(Directory.Exists(@"C:\inetpub\ftproot\UnitTest"));
            Directory.Delete(@"C:\inetpub\ftproot\UnitTest");
        }

        [TestMethod()]
        [TestCategory("File transfer")]
        public void UploadCnageNameDeleteFileFolder()
        {
            var client = GetFtpClient();
            client.Put(
                File.ReadAllBytes(
                    @"C:\Private projects\Core Framework\Stardust\Stardust.Core.CrossCuttingTest\TestFiles\utf8TestFile.txt"),
                "utf8TestFile.txt");
            Assert.IsTrue(File.Exists(@"C:\inetpub\ftproot\utf8TestFile.txt"));
            client.Rename("utf8TestFile.txt", "TestFile.txt");
            Assert.IsTrue(File.Exists(@"C:\inetpub\ftproot\TestFile.txt"));
            client.Delete("TestFile.txt");
            Assert.IsFalse(File.Exists(@"C:\inetpub\ftproot\TestFile.txt"));
        }

        /// <summary>
        ///A test for Create
        ///</summary>
        [TestMethod()]
        [TestCategory("File transfer")]
        public void CreateFtpReadTest1()
        {
            var type = "Ftp";
            var expected = typeof(FtpTrasfer);
            var actual = TransferFactory.Create(type)
                .SetServerRootUrl("127.0.0.1")
                //.SetUserNameAndPassword(GetUserName(), GetPassword())
                .SetFileName("/test/test.txt");
            var file = actual.Read();
            Assert.IsInstanceOfType(actual, expected);
            Assert.IsTrue(file.Length > 0);
        }

        [TestMethod]
        [TestCategory("File transfer")]
        public void GetListOfImplementationsTest()
        {
            var actual = TransferFactory.GetAvailableImplementations();
            Assert.IsTrue(actual.Count() >= 3);
            Assert.AreEqual("Http", actual.First().Key);
        }

        /// <summary>
        ///A test for Create
        ///</summary>
        [TestMethod()]
        [TestCategory("File transfer")]
        public void CreateTest()
        {
            var expected = new FileTransfer(); // TODO: Initialize to an appropriate value
            var actual = TransferFactory.Create();
            Assert.AreSame(expected.GetType(), actual.GetType());
        }

        [TestMethod()]
        [TestCategory("File transfer")]
        public void AddNewMethodTest()
        {
            TransferFactory.AddNewMethod<FileTransfer>("test");
            IFileTransfer expected = new FileTransfer();
            var actual = TransferFactory.Create("test");
            Assert.AreSame(expected.GetType(), actual.GetType());
        }

        [TestMethod()]
        [TestCategory("File transfer")]
        public void DirTest()
        {
            var client = GetFtpClient();
            var dirs = client.Dir();
            Assert.IsTrue(ArrayExtensions.ContainsElements(dirs));
            client.Quit();
        }

        [TestMethod()]
        [TestCategory("File transfer")]
        public void DirTestFolderTest()
        {
            var client = GetFtpClient();
            var dirs = client.Dir("test");
            Assert.IsTrue(ArrayExtensions.ContainsElements(dirs));
            Assert.AreEqual("Test.txt", dirs.First());
            client.Quit();
        }

        [TestMethod()]
        [TestCategory("File transfer")]
        public void SystemTest()
        {
            var client = GetFtpClient();
            var system = client.System();
            Assert.IsTrue(system.ContainsCharacters());
            Assert.AreEqual("Windows_NT", system);
            client.Quit();
        }

        [TestMethod()]
        [TestCategory("File transfer")]
        public void HelpTest()
        {
            var client = GetFtpClient();
            var helpText = client.Help(FtpClient.PwdCommand);
            Assert.IsTrue(helpText.ContainsCharacters());
            Assert.AreEqual("Syntax: PWD - (return current directory)", helpText);
            client.Quit();
        }

        [TestMethod()]
        [TestCategory("File transfer")]
        public void CreateCustomMethodTest()
        {
            try
            {
                TransferFactory.Create(TransferMethods.Custom);
                Assert.Fail();
            }
            catch
            {
                TransferFactory.BindCustomMethod<FileTransfer>();
                var custom = TransferFactory.Create(TransferMethods.Custom);
                Assert.IsInstanceOfType(custom, typeof(FileTransfer));
            }
        }

        private static FtpClient GetFtpClient()
        {
            var client = new FtpClient("127.0.0.1");
            Login(client);
            return client;
        }

        private static void Login(FtpClient client)
        {
            client.Login(
                GetUserName(),
                GetPassword());
        }

        private static string GetPassword()
        {
            return EncodingFactory.ReadFileText(
                @"C:\Private projects\Core Framework\Stardust\Stardust.Core.CrossCuttingTest\TestFiles\ttx.txt").Decrypt(new EncryptionKeyContainer("thisisthekey"));
        }

        private static string GetUserName()
        {
            return EncodingFactory.ReadFileText(
                @"C:\Private projects\Core Framework\Stardust\Stardust.Core.CrossCuttingTest\TestFiles\tty.txt");
        }

        private string GetHttpUserName()
        {
            FileName = @"C:\Private projects\Core Framework\Stardust\Stardust.Core.CrossCuttingTest\TestFiles\ttz.txt";
            alternateFileName = @"C:\Private projects\Core Framework\Stardust\Stardust.Core.CrossCuttingTest\TestFiles\tty.txt";
            if (File.Exists(FileName))
                return EncodingFactory.ReadFileText(FileName);
            return EncodingFactory.ReadFileText(alternateFileName);
        }
    }
}