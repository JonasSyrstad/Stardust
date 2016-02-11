//
// PoolContainer.cs
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
using System.ComponentModel;
using System.Threading.Tasks;
using Stardust.Interstellar;
using Stardust.Particles;

namespace Stardust.Core.Pool
{
    public sealed class PoolContainer<T> : PoolContainerBase
        where T : PoolableBase, new()
    {
        internal PoolContainer(int poolSize, Action<T> initializationCode)
            : base(poolSize)
        {
            InitializationCode = initializationCode;
            InitializePool();
        }

        internal PoolContainer(int poolSize)
            : this(poolSize, null)
        {
        }

        public Action<T> InitializationCode { get; internal set; }

        protected override void InitializePool()
        {
            lock (Pool)
            {
                if (PoolInitialized)
                {
                    return;
                }
                CreatePoolItems();
                PoolInitialized = true;
            }
        }

        private void CreatePoolItems()
        {
            for (var i = 0; i < PoolSize; i++)
            {
                var poolItem = new T { Used = false, Id = i };
                if (InitializationCode.IsInstance())
                {
                    InitializationCode(poolItem);
                }
                poolItem.Container = this;
                Pool.Enqueue(poolItem);
            }
        }

        public Task<T> GetItemFromPoolAsync()
        {
            return RuntimeFactory.Run(() => GetItemFromPool());
        }

        public T GetItemFromPool()
        {
            if (PoolSuspended)
            {
                throw new InvalidAsynchronousStateException("Pool is suspended for dispose operation");
            }
            PoolableBase item;
            var waitTime = GetTimeout();
            while (!Pool.TryDequeue(out item))
            {
                waitTime--;
                if (waitTime < 1)
                {
                    throw new TimeoutException("Get item from pool timed out");
                }
                Task.Delay(5).Wait();
            }
            item.Used = true;
            InUse++;
            return (T)item;
        }

        internal override void ReturnToPool(PoolableBase poolableBase)
        {
            InUse--;
            poolableBase.Used = false;
            Pool.Enqueue(poolableBase);
        }

        internal PoolStatus GetStatus()
        {
            return new PoolStatus
                   {
                       Used = InUse,
                       Free = PoolSize - InUse,
                       Size = PoolSize,
                       Initialized = PoolInitialized,
                       Suspended = PoolSuspended
                   };
        }
    }
}