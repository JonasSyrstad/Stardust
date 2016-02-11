//
// CSharpInjectorTest.cs
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
using Stardust.Core.DynamicCompiler;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
	/// <summary>
	///This is a test class for CSharpInjectorTest and is intended
	///to contain all CSharpInjectorTest Unit Tests
	///</summary>
	[TestClass()]
	public class CSharpInjectorTest
	{
        private IKernelContext KernelScope;

        [TestInitialize]
        public void Initialize()
        {
            KernelScope = Resolver.BeginKernelScope();
            Resolver.LoadModuleConfiguration<Blueprint>();
            Resolver.GetConfigurator().Bind<ICodeInjector>().To<CSharpInjector>(LanguageType.CSharp);
            Resolver.GetConfigurator().Bind<ICodeInjector>().To<VbCodeInjector>(LanguageType.VisualBasic);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Resolver.EndKernelScope(KernelScope, true);
        }

        const string CsCode = @"public class DynamicTestInterface : Stardust.Core.CrossCuttingTest.LegacyTests.IDynamicTestInterface
									{
										public int HelloWorld()
										{
											return 666;
										}
									}";
		const string VbCode = @"Public Class DynamicTestInterface
								Implements Stardust.Core.CrossCuttingTest.LegacyTests.IDynamicTestInterface
								Public Function HelloWorld() As Integer Implements Stardust.Core.CrossCuttingTest.LegacyTests.IDynamicTestInterface.HelloWorld
									Return 666
								End Function
							End Class";

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


		[TestMethod]
        [TestCategory("Compiler")]
		public void CompileAndCreateInstanceTestCS()
		{
			var target = CompilatorFactory.CreateCompiler(LanguageType.CSharp);
			target.AddReference("stardust.core.crosscuttingtest.dll");
			var expected = 666;
			var actual = target.CompileAndCreateInstance<IDynamicTestInterface>(CsCode);
			Assert.AreEqual(expected, actual.HelloWorld());
		}

		[TestMethod]
        [TestCategory("Compiler")]
		public void CompileAndExecuteMethodTestCS()
		{
			var target = CompilatorFactory.CreateCompiler(LanguageType.CSharp);
            target.AddReference("stardust.core.crosscuttingtest.dll");

			var expected = 666;
			var actual = target.CompileAndExecute(CsCode, "DynamicTestInterface.HelloWorld");
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
        [TestCategory("Compiler")]
		public void CompileAndCreateInstanceTestVB()
		{
			var target = CompilatorFactory.CreateCompiler(LanguageType.VisualBasic);
            target.AddReference("stardust.core.crosscuttingtest.dll");
			var expected = 666; // TODO: Initialize to an appropriate value
			var actual = target.CompileAndCreateInstance<IDynamicTestInterface>(VbCode);
			Assert.AreEqual(expected, actual.HelloWorld());
		}

		[TestMethod]
        [TestCategory("Compiler")]
		public void CompileAndExecuteMethodTestVB()
		{
			var target = CompilatorFactory.CreateCompiler(LanguageType.VisualBasic);
            target.AddReference("stardust.core.crosscuttingtest.dll");
			var expected = 666;
			var actual = target.CompileAndExecute(VbCode, "DynamicTestInterface.HelloWorld");
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
        [TestCategory("Compiler")]
		public void StringGetHashTest()
		{
			var target1 = "Dette er en test";
			var target2 = "Dette er en test";
			var target3 = "Dette er en test!";
			Assert.AreEqual(target1.GetHashCode(), target2.GetHashCode());
			Assert.AreNotEqual(target1.GetHashCode(), target3.GetHashCode());
		}
	}

	public interface IDynamicTestInterface
	{
		int HelloWorld();
	}
}
