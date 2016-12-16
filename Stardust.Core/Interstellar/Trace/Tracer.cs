//
// tracer.cs
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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar.Trace
{
    internal class Tracer : ITracer
    {
        private TraceHandler Handler;
        private bool ExtendedLifeTime;
        private ManualResetEvent ReleaseHandle;

        private Stopwatch timer;

        public CallStackItem ParentItem { get; private set; }

        public ITracer StartTrace(string taskName, string methodName)
        {
            timer = Stopwatch.StartNew();
            MyCallstackItem = new CallStackItem
                              {
                                  CallStack = new List<CallStackItem>(),
                                  Name = taskName,
                                  MethodName = methodName,
                                  TimeStamp = DateTime.Now
                              };
            if (ParentItem.IsInstance())
                ParentItem.CallStack.Add(MyCallstackItem);
            return this;
        }

        public ITracer StartTrace(string taskName, MethodBase method)
        {
            return StartTrace(taskName, method.Name);
        }

        public ITracer StartTrace(Type task, MethodBase method)
        {
            return StartTrace(task.FullName, method.Name);
        }

        public ITracer StartTrace(Type task, string methodName)
        {
            return StartTrace(task.FullName, methodName);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool isDisposing)
        {
            try
            {
                if (ExtendedLifeTime) return;

                if (isDisposing)
                {
                    GC.SuppressFinalize(this);
                    var errorCode = Marshal.GetExceptionCode();
                    if (errorCode != 0)
                    {
                        Exception exception = null;
                        try
                        {
                            exception = Marshal.GetExceptionForHR(errorCode);
                        }
                        catch (Exception)
                        {

                        }
                        SetErrorState(exception);
                    }
                }
                if (Handler.IsInstance())
                {
                    if (ParentItem.IsNull()) Handler.Dispose();
                    else
                    {
                        Handler.CurrentTracer = ParentTracer;
                    }
                }
                timer.Stop();
                IsDisposed = true;
                SetTimings();
                if (ReleaseHandle.IsInstance())
                {
                    ReleaseHandle.Set();
                }
            }
            catch
            {
            }
        }

        private void SetTimings()
        {
            if (MyCallstackItem.IsNull()) return;
            MyCallstackItem.EndTimeStamp = DateTime.Now;
            MyCallstackItem.ExecutionTime = timer.ElapsedMilliseconds;
        }


        public ITracer CreateChildTracer()
        {
            if (MyCallstackItem.IsNull()) throw new StardustCoreException("Tracer not initialized.");
            var tracer = new Tracer { ParentItem = MyCallstackItem, ParentTracer = this };
            return tracer;
        }

        public CallStackItem GetCallstack()
        {
            return MyCallstackItem;
        }

        public void SetHandler(TraceHandler handler)
        {
            Handler = handler;
        }

        public bool IsDisposed { get; private set; }

        public void AppendCallstack(CallStackItem callStack)
        {
            if(MyCallstackItem.IsNull()||MyCallstackItem.CallStack.IsNull()) return;
            MyCallstackItem.CallStack.Add(callStack);
        }

        public void SetErrorState(Exception ex)
        {
            if(Handler==null) return;
            if (Handler.ErrorStateSet) return;
            var builder = new StringBuilder();
            GetTrace(builder);
            Handler?.RootTracer?.SetFaileurePathInformation(builder.ToString());
            if (ex.IsInstance())
                Handler?.RootTracer?.SetException(ex);
            Handler.ErrorStateSet = true;
        }

        public void GetTrace(StringBuilder builder)
        {
            if (ParentItem != null)
                ParentTracer.GetTrace(builder); 
            if (ParentTracer.IsInstance())
                builder.Append(".");
            if (MyCallstackItem != null)
                builder.Append($"[{MyCallstackItem.Name}.{MyCallstackItem.MethodName}]");
        }

        public void SetException(Exception exception)
        {
            if (exception.IsNull()) return;
            ;
            LastException = exception;
            MyCallstackItem.ExceptionMessage = exception.Message;
            MyCallstackItem.StackTrace = exception.StackTrace;
        }

        public Exception GetLastException()
        {
            return LastException;
        }

        public void SetFaileurePathInformation(string errorPath)
        {
            MyCallstackItem.ErrorPath = errorPath;
        }

        public IDisposable ExtendLifeTime()
        {
            ExtendedLifeTime = true;
            ReleaseHandle = new ManualResetEvent(false);
            return new ExtendedTracerScope(this);
        }

        public bool HasExtendedLife()
        {
            return ExtendedLifeTime;
        }

        public ITracer WrapTracerResult()
        {
            SetTimings();
            return this;
        }

        public CallStackItem MyCallstackItem { get; private set; }

        public ITracer SetAdidtionalInformation(string info)
        {
            MyCallstackItem.AdditionalInformation = info;
            return this;
        }

        ~Tracer()
        {
            Dispose(false);
        }

        public ITracer ParentTracer { get; set; }

        public Exception LastException { get; private set; }

        public void EndExtednedLife()
        {
            ExtendedLifeTime = false;
        }

        public ManualResetEvent GetWaitHandle()
        {
            
            return ReleaseHandle;
        }
    }

    internal class ExtendedTracerScope : IDisposable
    {
        private readonly Tracer Tracer;

        public ExtendedTracerScope(Tracer tracer)
        {
            ContainerFactory.Current.Bind(typeof(ManualResetEvent),tracer.GetWaitHandle(),Scope.Context);
            Tracer = tracer;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            Tracer.EndExtednedLife();
            Tracer.Dispose();
        }

        ~ExtendedTracerScope()
        {
            Dispose(false);
        }
    }
}