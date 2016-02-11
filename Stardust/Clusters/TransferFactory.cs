//
// TransferFactory.cs
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
using System.Collections.Generic;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Particles.FileTransfer;

namespace Stardust.Clusters
{
    public static class TransferFactory
    {
        private const string NotDefinedMessage = "{0} is not defined. Please bind this method to an implementation";

        //static TransferFactory()
        //{
        //    BindAllImplementations();
        //}

        //private static void BindAllImplementations()
        //{
        //    Resolver.Bind<IFileTransfer>().To<FileTransfer>(TransferMethods.File);
        //    Resolver.Bind<IFileTransfer>().To<HttpFileTrasfer>(TransferMethods.Http);
        //    Resolver.Bind<IFileTransfer>().To<FtpTrasfer>(TransferMethods.Ftp);
        //    Resolver.Bind<IFileTransfer>().To<FileTransfer>();
        //}

        public static IFileTransfer Create()
        {
            return Resolver.Activate<IFileTransfer>();
        }

        public static IFileTransfer Create(TransferMethods type)
        {
            try
            {
                return Create(type.ToString());
            }
            catch (Exception ex)
            {
                throw new StardustCoreException(String.Format(NotDefinedMessage,type),ex);
            }
        }

        public static IFileTransfer Create(string type)
        {
            return Resolver.Activate<IFileTransfer>(type);
        }

        public static void AddNewMethod<T>(string typeName) where T : IFileTransfer
        {
            Resolver.GetConfigurator().Bind<IFileTransfer>().To<T>(typeName);
        }

        public static void BindCustomMethod<T>() 
            where T : IFileTransfer
        {
            Resolver.GetConfigurator().Bind<IFileTransfer>().To<T>(TransferMethods.Custom);
        }

        public static IEnumerable<KeyValuePair<string, string>> GetAvailableImplementations()
        {
            return Resolver.GetImplementationsOf<IFileTransfer>();
        }
    }
}
