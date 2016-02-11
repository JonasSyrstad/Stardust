//
// WorkflowTest.cs
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
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Core.Default.Implementations;
using Stardust.Core.Workflow;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;
using Stardust.Particles.FileTransfer;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    [TestClass]
    public class WorkflowTest
    {

        private IKernelContext KernelScope;

        [TestInitialize]
        public void Initialize()
        {
            KernelScope = Resolver.BeginKernelScope();
            Resolver.LoadModuleConfiguration<Blueprint>();
            Resolver.GetConfigurator().Bind<IFileTransfer>().To<FileTransfer>(TransferMethods.File);
            Resolver.GetConfigurator().Bind<IFileTransfer>().To<HttpFileTrasfer>(TransferMethods.Http);
            Resolver.GetConfigurator().Bind<IFileTransfer>().To<FtpTrasfer>(TransferMethods.Ftp);
            Resolver.GetConfigurator().Bind<IFileTransfer>().To<FileTransfer>();
            Resolver.GetConfigurator().UnBind<ILogging>().AllAndBind().To<Log4NetLogging>();
            Logging.ResetLogger();

        }

        [TestCleanup]
        public void Cleanup()
        {
            Resolver.EndKernelScope(KernelScope, true);
        }

        [TestMethod]
        [TestCategory("WF helpers")]
        [DeploymentItem("Workflows/Workflow.xaml")]
        public void TestWorkflowRunner()
        {
            var expected = "Run complete";  
            WorkflowFactory.Using<WorkflowTest>();
            var result = WorkflowFactory.RunXaml("Workflow.xaml", new Dictionary<string, object>());
            Assert.IsTrue(result.IsInstance());
            Assert.AreEqual(expected, result["argument1"]);
        }

        [TestMethod]
        [TestCategory("WF helpers")]
        [DeploymentItem("Workflows/Transfer.xaml")]
        public void TestReadTransferActivity()
        {
            WorkflowFactory.Using("Stardust.Core.CrossCuttingTest");
            var xamlBuffer = File.ReadAllBytes("Transfer.xaml");
            var result = xamlBuffer.CreateWorkflowContainer<OutParameterContainerBase>().RunWorkflow();
            Assert.IsTrue(result.IsInstance());
            Assert.IsInstanceOfType(result.File, typeof(byte[]));
        }

        [TestMethod]
        [TestCategory("WF helpers")]
        [DeploymentItem("Workflows/Write.xaml")]
        public void TestWriteTransferActivity()
        {
            var buffer = File.ReadAllBytes(@"C:\temp\logging\log.txt");
            WorkflowFactory.Using("Stardust.Core.CrossCuttingTest");
            var container = WorkflowFactory.CreateWorkflowContainer<TestContainerBase>();
            container.File = buffer;
            var result = container.RunWorkflow();
            Assert.IsTrue(result.OutArguments.IsInstance());
            if (File.Exists(@"C:\temp\logging\testFile.txt"))
            {
                Assert.IsTrue(true);
                File.Delete(@"C:\temp\logging\testFile.txt");
            }
            else
                Assert.Fail();
        }
    }

    public class OutParameterContainerBase:WorkflowContainerBase
    {

        public override string XamlPath
        {
            get { return "Transfer.xaml"; }
        }

        public byte[] File
        {
            get { return OutArguments["argument1"] as byte[]; }
        }

        protected override bool IsValid()
        {
            return true;
        }
    }
}
