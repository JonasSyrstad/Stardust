//
// itracer.cs
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
using System.Reflection;
using System.Text;

namespace Stardust.Stellar
{
    public interface ITracer : IDisposable
    {
        CallStackItem ParentItem { get; }

        ITracer StartTrace(string taskName, string methodName);

        ITracer StartTrace(string taskName, MethodBase method);

        ITracer StartTrace(Type task, MethodBase method);

        ITracer StartTrace(Type task, string methodName);

        ITracer SetAdidtionalInformation(string info);

        ITracer CreateChildTracer();

        CallStackItem GetCallstack();

        void SetHandler(TraceHandler handler);

        bool IsDisposed { get; }

        void AppendCallstack(CallStackItem callStack);

        void SetErrorState(Exception ex);
        void GetTrace(StringBuilder builder);

        void SetException(Exception exception);

        Exception GetLastException();

        void SetFaileurePathInformation(string errorPath);

        IDisposable ExtendLifeTime();
        bool HasExtendedLife();

        ITracer WrapTracerResult();
    }
}
