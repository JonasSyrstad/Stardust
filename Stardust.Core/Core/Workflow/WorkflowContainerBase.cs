//
// WorkflowContainerBase.cs
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Stardust.Particles;
using Stardust.Particles.Collection.Arrays;

namespace Stardust.Core.Workflow
{
    /// <summary>
    /// Override this to create a strong typed class for in/out arguments
    /// Override IsValid() to provide more argument validation.
    /// </summary>
    public abstract class WorkflowContainerBase : IDisposable
    {
        private const string LocalAssemblyNotSet = "Local assembly not set. Please call WorkflowRunner.Using to register local assembly.";
        private const string InArgumentsIsNull = "InArguments is null";

        /// <summary>
        /// reference to the xaml file on disk, if XamlStream is set this will not be used.
        /// </summary>
        public abstract string XamlPath { get; }

        /// <summary>
        /// This propety contains a xaml stream and will override the XamlPath property
        /// </summary>
        /// <remarks>very usefull to override the default workflow with workflows stored in the database</remarks>
        public Stream XamlStream { get; set; }

        /// <summary>
        /// Creates and set the XamlStream property from an xaml xml string. 
        /// </summary>
        public void SetXamlStreamFromXamlXmlString(string xaml)
        {
            if (xaml.IsNullOrWhiteSpace()) return;
            var byteArray = xaml.GetByteArray();
            XamlStream = byteArray.ToStream();
        }

        public void SetXamlStreamFromBuffer(byte[] buffer)
        {
            if (ArrayExtensions.ContainsElements(buffer))
                XamlStream = buffer.ToStream();
        }

        public void ClearStream()
        {
            if (XamlStream.IsInstance())
                XamlStream.Dispose();
            XamlStream = null;
        }

        protected internal IDictionary<string, object> InArguments { get; set; }

        public IDictionary<string, object> OutArguments { get; set; }

        /// <summary>
        ///Override this to add additional validation of the container attributes
        /// </summary>
        [ExcludeFromCodeCoverage]
        protected virtual bool IsValid()
        {
            return true;
        }

        internal bool Valid()
        {
            if (WorkflowFactory.LocalAssembly.IsNull())
            {
                ValidationMessage = LocalAssemblyNotSet;
                return false;
            }
            if (InArguments.IsNull())
            {
                ValidationMessage = InArgumentsIsNull;
                return false;
            }
            return IsValid();
        }

        public string ValidationMessage { get; protected set; }

        public void Dispose()
        {
            ClearStream();
            GC.SuppressFinalize(this);
        }

        ~WorkflowContainerBase()
        {
            ClearStream();
        }
    }
}