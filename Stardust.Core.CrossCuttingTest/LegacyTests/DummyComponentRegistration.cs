//
// DummyComponentRegistration.cs
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
using Stardust.Nucleus;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    public class DummyComponentRegistration : IComponentRegistration
    {
        public IComponentRegistration Enumerate()
        {
            return this;
        }

        public ObjectInitializer[] GetAllRegisteredImplementationsFor<T>()
        {
            throw new NotImplementedException();
        }

        public ObjectInitializer GetObjectInitializerFor<T>(string moduleName)
        {
            throw new NotImplementedException();
        }

        public IComponentRegistration ImportAssemblyWith<T>(string registrationName, byte[] assembly)
        {
            throw new NotImplementedException();
        }

        public IComponentRegistration ImportAssemblyWith<T>(string registrationName, byte[] assembly, string className)
        {
            throw new NotImplementedException();
        }

        public IComponentRegistration ImportAssemblyWith(Type implementationToRegister, string registrationName, byte[] assembly)
        {
            throw new NotImplementedException();
        }

        public IComponentRegistration ImportAssemblyWith(Type implementationToRegister, string registrationName, byte[] assembly, string className)
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized()
        {
            return true;
        }

        public IComponentRegistration ScanImportedAssemblyFor<T>(string registrationName)
        {
            throw new NotImplementedException();
        }

        public IComponentRegistration ScanImportedAssemblyFor<T>(string registrationName, string className)
        {
            throw new NotImplementedException();
        }

        public IComponentRegistration ScanImportedAssemblyFor(Type implementationToRegister, string registrationName)
        {
            throw new NotImplementedException();
        }

        public IComponentRegistration ScanImportedAssemblyFor(Type implementationToRegister, string registrationName, string className)
        {
            throw new NotImplementedException();
        }

        public IComponentRegistration SetFolder(string path)
        {
            return this;
        }

        public IComponentRegistration Update()
        {
            throw new NotImplementedException();
        }
    }
}