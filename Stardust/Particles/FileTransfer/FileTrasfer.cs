//
// FileTrasfer.cs
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Stardust.Particles.FileTransfer
{
    public sealed class FileTransfer :DynamicConfigurableObjectBase, IFileTransfer, IDisposable
    {
        private string UserName;
        private string Password;
        private ImpersonateUser ImpersonationHandler;

        public FileTransfer()
            : base(null)
        {
        }

        public IFileTransfer SetServerRootUrl(string url)
        {
            Url = url;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IFileTransfer SetPort(string port)
        {
            return this;
        }

        [ExcludeFromCodeCoverage]
        public IFileTransfer SetTimeout(decimal ttl)
        {
            return this;
        }

        [ExcludeFromCodeCoverage]
        public bool TryTransfer(byte[] data)
        {
            try
            {
                Transfer(data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        [ExcludeFromCodeCoverage]
        public void Transfer(byte[] data)
        {
            try
            {
                Impersonate();
                File.WriteAllBytes(Url, data);
            }
            finally
            {
                EndImpersonation();
            }
        }

        private void EndImpersonation()
        {
            if (ImpersonationHandler.IsNull()) return;
            ImpersonationHandler.Undo();
            ImpersonationHandler.TryDispose();
            ImpersonationHandler = null;
        }

        private void Impersonate()
        {
            if (UserName.IsNullOrEmpty() || Password.IsNullOrEmpty()) return;
            ImpersonationHandler = new ImpersonateUser();
            var domainUser = UserName.Split('\\');
            var userPart = UserName;
            var domainPart = "";
            if (domainUser.Length == 2)
            {
                userPart = domainUser[1];
                domainPart = domainUser[0];
            }
            ImpersonationHandler.Impersonate(domainPart, userPart, Password);
        }

        private string Url1;
        public string Url
        {
            get { return Url1 + FileName; }
            private set { Url1 = value; }
        }

        public byte[] Read()
        {
            if (!File.Exists(Url)) throw new StardustCoreException("File does not exist");
            try
            {
                Impersonate();
                return File.ReadAllBytes(Url);
            }
            finally
            {
                EndImpersonation();
            }
        }

        public IFileTransfer SetUserNameAndPassword(string userName, string password)
        {
            UserName = userName;
            Password = password;
            return this;
        }

        public IFileTransfer SetFileName(string fileName)
        {
            FileName = fileName;
            return this;
        }

        public string FileName { get; private set; }

        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            EndImpersonation();
            GC.SuppressFinalize(this);
        }

        ~FileTransfer()
        {
            EndImpersonation();
        }
    }
}