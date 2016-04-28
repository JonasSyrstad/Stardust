//
// AssemblyInfo.cs
// This file is part of Stardust
//
// Author: Jonas Syrstad (jsyrstad2+StardustCore@gmail.com), http://no.linkedin.com/in/jonassyrstad/) 
// Copyright (c) 2014 Jonas Syrstad. All rights reserved.
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

using System.Reflection;
using System.Runtime.InteropServices;
using Stardust.Core;

[assembly:AssemblyTitle("Stardust.Core")]
[assembly:AssemblyDescription("A framework for building large scalable distributed applications with ease")]
[assembly: AssemblyConfiguration("")]
[assembly:AssemblyCompany("Stardust AS")]
[assembly: AssemblyProduct("StardustCoreLib")]
[assembly:AssemblyCopyright("Copyright © Stardust 2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("56c33bef-901d-4bdb-bbe7-d6d8d3ebabfc")]

[assembly:AssemblyVersion("2.1.1.132")]

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(FrameworkInitializer), "InitializeModules")]

