//
// RuntimeFactory.cs
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
using System.Threading.Tasks;
using Stardust.Core;
using Stardust.Interstellar.Tasks;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    public static class RuntimeFactory
    {
        /// <summary>
        /// If Scope.PerRequest is specified the instance is created in an extended Context scope. that is, if it has not been used before in the scope a new will be created each time.
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static IRuntime CreateRuntime(Scope scope)
        {
            if (scope == Scope.Context)
                return CreateRuntime();
            return Resolver.Activate<IRuntime>(scope);
        }

        public static IRuntime Current
        {
            get
            {
                try
                {
                    if (SynchronizationContext.Current is ThreadSynchronizationContext)
                        return CreateRuntime();
                    else return CreateRuntime(Scope.PerRequest);
                }
                catch (Exception ex)
                {
                    ex.Log("No runtime found. Make sure the framework has loaded the blueprint file.");
                    return null;
                }
            }
        }

        public static IRuntime CreateRuntime()
        {
            return Resolver.Activate<IRuntime>();
        }

        public static void RecycleWebProcess(ProsessType allAppPoolsOnServer = ProsessType.Current)
        {
            if (allAppPoolsOnServer == ProsessType.AllApplicationPools)
                ApplicationPoolRecycle.RecycleAll();
            else
                ApplicationPoolRecycle.RecycleCurrentApplicationPool();
        }

        public static Guid GetInstanceId()
        {
            var id = (Guid?)ContainerFactory.Current.Resolve(typeof(Guid?), Scope.Context, GetNewGuid);
            return id.Value;
        }

        private static object GetNewGuid()
        {
            return Guid.NewGuid();
        }

        internal static bool TryGetSupportCode(this IRuntime runtime, out string supportCode)
        {
            try
            {
                if (runtime == null || runtime.GetStateStorageContainer() == null)
                {
                    supportCode = null;
                    return false;
                }
                StateStorageItem item;
                var result = runtime.GetStateStorageContainer().TryGetItem("supportCode", out item);
                if (item != null && item.Value != null) supportCode = (string)item.Value;
                else
                {
                    supportCode = null;
                    return false;
                }
                return result;
            }
            catch (Exception ex)
            {
                ex.Log();
                supportCode = null;
                return false;
            }
        }

        public static bool TrySetSupportCode(this IRuntime runtime, string supportCode)
        {
            try
            {
                if (runtime == null || runtime.GetStateStorageContainer() == null)
                {
                    return false;
                }
                var result = runtime.GetStateStorageContainer().TryAddStorageItem(supportCode, "supportCode");
                return result;
            }
            catch (Exception ex)
            {
                ex.Log();
                return false;
            }
        }

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a Task handle for that work using the current synchronizationContext allowing the Tracer feature and other context aware things to work as intended.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task Run( Action action)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a Task handle for that work using the current synchronizationContext allowing the Tracer feature and other context aware things to work as intended.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task<T> Run<T>(Func<T> action)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a Task handle for that work using the current synchronizationContext allowing the Tracer feature and other context aware things to work as intended.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task Run(this IRuntime runtime, Action action)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a Task handle for that work using the current synchronizationContext allowing the Tracer feature and other context aware things to work as intended.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task<T> Run<T>(this IRuntime runtime, Func<T> action)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

       
    }
}