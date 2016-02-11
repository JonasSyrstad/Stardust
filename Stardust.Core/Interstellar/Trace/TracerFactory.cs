//
// TracerFactory.cs
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
using System.Threading;
using Stardust.Core;
using Stardust.Core.Wcf;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar.Trace
{
    public static class TracerFactory
    {
        private static bool DoLogging
        {
            get { return ConfigurationManagerHelper.GetValueOnKey("stardust.Debug") == "true"; }
        }

        
        public static ITracer StartTracer(object callingInstance, string taskName, string methodName)
        {
            if (DoLogging)
            {
                Logging.DebugMessage("Current sync context: {0}", SynchronizationContext.Current);
                if (!(SynchronizationContext.Current is ThreadSynchronizationContext))
                    Logging.DebugMessage("not in stardust synchronized mode...");
            }
            ITracer newTracer;
            var handler = ContainerFactory.Current.Resolve(typeof(TraceHandler), Scope.Context) as TraceHandler;
            if (handler == null)
            {
                if (DoLogging) Logging.DebugMessage("Creating new trace handler for {0}.{1}", taskName, methodName);
                handler = new TraceHandler();
                ContainerFactory.Current.Bind(typeof(TraceHandler), handler, Scope.Context);
            }
            if (handler.RootTracer.IsNull())
            {
                handler.RootTracer = new Tracer();
                newTracer = handler.RootTracer;
                handler.CurrentTracer = newTracer;
            }
            else
            {
                newTracer = handler.CurrentTracer.CreateChildTracer();
                handler.CurrentTracer = newTracer;
            }
            newTracer.SetHandler(handler);
            return newTracer.StartTrace(taskName, methodName);
        }

        public static ITracer StartTracer(object callingInstance, Type task, string methodName)
        {
            return StartTracer(callingInstance, task.Name, methodName);
        }

        public static ITracer StartTracer(object callingInstance, string methodName)
        {
            return StartTracer(callingInstance, callingInstance.GetType().FullName, methodName);
        }

        public static ITracer StartTracer(Type callingInstance, string methodName)
        {
            return StartTracer(callingInstance, callingInstance, methodName);
        }
    }
}