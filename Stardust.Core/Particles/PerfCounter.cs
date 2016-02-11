//
// PerfCounter.cs
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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Stardust.Particles
{
    [ExcludeFromCodeCoverage]
    public static class PerfCounter
    {
        private static PerformanceCounter CpuCounter;
        private static PerformanceCounter TimeInGc;
        private static PerformanceCounter LargeHeap;

        public static PerformanceCounter GetTimeInGc()
        {
            if (TimeInGc.IsInstance()) return TimeInGc;
            TimeInGc = new PerformanceCounter
                           {
                               CategoryName = ".NET CLR Memory",
                               CounterName = "% Time in GC",
                               InstanceName = RuntimeHelper.GetProcessName()
                           };
            return TimeInGc;
        }

        public static PerformanceCounter GetLargeObjectHeap()
        {
            if (LargeHeap.IsInstance()) return LargeHeap;
            LargeHeap = new PerformanceCounter
                            {
                                CategoryName = ".NET CLR Memory",
                                CounterName = "Large Object Heap size",
                                InstanceName = RuntimeHelper.GetProcessName()
                            };
            return LargeHeap;
        }

        public static PerformanceCounter GetCPUCounter()
        {
            if (CpuCounter.IsInstance()) return CpuCounter;
            CpuCounter = new PerformanceCounter
                             {
                                 CategoryName = "Processor",
                                 CounterName = "% Processor Time",
                                 InstanceName = "_Total"
                             };
            return CpuCounter;
        }
    }
}
