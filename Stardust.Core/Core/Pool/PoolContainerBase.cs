//
// PoolContainerBase.cs
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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Interstellar;

namespace Stardust.Core.Pool
{
    /// <summary>
    /// Not intended for use or implementation outside this library
    /// </summary>
    public abstract class PoolContainerBase : IDisposable
    {
        private static int? Timeout;

        protected int GetTimeout()
        {
            if (!Timeout.HasValue)
                Timeout = GetTimeoutValue();
            return Timeout.Value;
        }

        private int? GetTimeoutValue()
        {
            var context = RuntimeFactory.CreateRuntime(Scope.PerRequest).Context;
            if (context.IsInstance())
            {
                var timeout = context.GetServiceConfiguration().GetConfigParameter("PoolAwaitTimeOut");
                if (timeout.ContainsCharacters())
                    return int.Parse(timeout);
            }
            return 200;
        }

        protected readonly ConcurrentQueue<PoolableBase> Pool;
        protected int InUse;
        protected bool PoolInitialized;
        protected int PoolSize;

        internal PoolContainerBase()
            : this(DefaultPoolSize())
        {
        }

        internal PoolContainerBase(int poolSize)
        {
            PoolSize = poolSize;
            Pool = new ConcurrentQueue<PoolableBase>();
        }

        public bool PoolSuspended { get; protected set; }

        public void Dispose()
        {
            DisposeAll(false);
            GC.SuppressFinalize(this);
        }

        private static int DefaultPoolSize()
        {
            var poolSize = ConfigurationManagerHelper.GetValueOnKey("defaultPoolSize");
            if (poolSize.ContainsCharacters())
            {
                return int.Parse(poolSize);
            }
            return 20;
        }

        internal abstract void ReturnToPool(PoolableBase poolableBase);

        public void DisposeAll(bool reinitialize)
        {
            lock (Pool)
            {
                DisposeAll();
            }
            if (reinitialize)
            {
                InitializePool();
            }
        }

        public Task DisposeAllAsync(bool reinitialize)
        {
            return RuntimeFactory.Run(() => DisposeAll(reinitialize));
        }

        protected virtual void DisposeAll()
        {
            while (InUse != 0) Thread.Sleep(1);
            if (Pool.Count != PoolSize) throw new IndexOutOfRangeException("All pool items has not been returned to the pool");
            if (!PoolInitialized) return;
            PoolSuspended = true;
            PoolableBase poolItem;
            while (Pool.TryDequeue(out poolItem))
            {
                if (poolItem == null) break;
                poolItem.Dispose(true);
                poolItem = null;
            }
            PoolInitialized = false;
            PoolSuspended = false;
        }

        protected abstract void InitializePool();

        ~PoolContainerBase()
        {
            DisposeAll(false);
        }
    }
}