﻿//
// ConnectionStringPoolContainer.cs
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
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Stardust.Particles;

namespace Stardust.Core.Pool
{
    internal class ConnectionStringPoolContainer<T> : PoolContainerBase where T : ConnectionStringPoolableBase, new()
    {
        private readonly string PoolTypeName;

        public Action<T> InitializationCode { get; internal set; }

        public ConnectionStringPoolContainer(int poolSize, Action<T> initializationCode)
        {
            PoolTypeName = typeof(T).FullName;
            PoolSize = poolSize;
            InitializationCode = initializationCode;
        }

        internal override void ReturnToPool(PoolableBase poolableBase)
        {
            var item = poolableBase as ConnectionStringPoolableBase;
            var pool = Pools[GetPoolName(item.ConnectionString)];
            var locker = GetLocker(item.ConnectionString);
            pool.Enqueue(item);
            item.Used = false;
            InUse--;
            locker.Release();
        }

        public async Task<T> GetItemFromPoolAsync(string connectionString)
        {
            if (PoolSuspended)
            {
                throw new InvalidAsynchronousStateException("Pool is suspended for dispose operation");
            }
            ConnectionStringPoolableBase item;
            ConcurrentQueue<ConnectionStringPoolableBase> pool;
            if (!Pools.TryGetValue(GetPoolName(connectionString), out pool))
            {
                pool = InitializePool(connectionString);
            }
            var locker = GetLocker(connectionString);
            if (!await locker.WaitAsync(GetTimeout()).ConfigureAwait(false)) throw new TimeoutException("Get item from pool timed out");
            var waitTime = GetTimeout();
            while (!pool.TryDequeue(out item))
            {
                waitTime--;
                if (waitTime < 1)
                {
                    throw new TimeoutException("Get item from pool timed out");
                }
                await Task.Delay(1);
            }
            item.Used = true;
            InUse++;
            WritePoolDepletionWaring<T>(connectionString, locker);
            return (T)item;
        }

        public T GetItemFromPool(string connectionString)
        {
            if (PoolSuspended)
            {
                throw new InvalidAsynchronousStateException("Pool is suspended for dispose operation");
            }
            ConnectionStringPoolableBase item;
            ConcurrentQueue<ConnectionStringPoolableBase> pool;
            if (!Pools.TryGetValue(GetPoolName(connectionString), out pool))
            {
                pool = InitializePool(connectionString);
            }
            var locker = GetLocker(connectionString);
            if (!locker.Wait(GetTimeout())) throw new TimeoutException("Get item from pool timed out");
            var waitTime = GetTimeout();
            while (!pool.TryDequeue(out item))
            {
                waitTime--;
                if (waitTime < 1)
                {
                    throw new TimeoutException("Get item from pool timed out");
                }
                Thread.Sleep(1);
            }
            item.Used = true;
            InUse++;
            WritePoolDepletionWaring<T>(connectionString, locker);

            return (T)item;
        }

        private void WritePoolDepletionWaring<T>(string connectionString, SemaphoreSlim locker)
        {
            if (((double)(PoolSize - locker.CurrentCount) / PoolSize) > 0.80)
            {
                Logging.DebugMessage("Pool {0} nearing depletion {1}/{2} pooled items used", typeof(T).FullName, (PoolSize - locker.CurrentCount), PoolSize);
            }
        }

        private SemaphoreSlim GetLocker(string connectionString)
        {
            SemaphoreSlim locker;
            if (!Semaphores.TryGetValue(GetPoolName(connectionString), out locker))
            {
                locker = new SemaphoreSlim(PoolSize);
                Semaphores.TryAdd(GetPoolName(connectionString), locker);
            }
            return locker;
        }

        private ConcurrentQueue<ConnectionStringPoolableBase> InitializePool(string connectionString)
        {
            lock (Pools)
            {
                if (Pools.ContainsKey(GetPoolName(connectionString))) return Pools[GetPoolName(connectionString)];
                var pool = new ConcurrentQueue<ConnectionStringPoolableBase>();
                if (Pools.ContainsKey(GetPoolName(connectionString))) return Pools[GetPoolName(connectionString)];
                CreatePoolItems(pool, connectionString);
                PoolInitialized = true;
                Pools.AddOrUpdate(GetPoolName(connectionString), pool);
                return pool;
            }
        }

        private void CreatePoolItems(ConcurrentQueue<ConnectionStringPoolableBase> pool, string connectionString)
        {
            for (var i = 0; i < PoolSize; i++)
            {
                var poolItem = new T { Used = false, Id = i, ConnectionString = connectionString };
                if (InitializationCode.IsInstance())
                {
                    InitializationCode(poolItem);
                }
                poolItem.Container = this;
                pool.Enqueue(poolItem);
            }
        }

        internal string GetPoolName(string connectionString)
        {
            return string.Format("{0}_{1}", PoolTypeName, connectionString);
        }

        protected override void InitializePool()
        {
        }

        protected override void DisposeAll()
        {
            if (!PoolInitialized) return;
            PoolSuspended = true;
            foreach (var pool in Pools)
            {
                ConnectionStringPoolableBase poolItem;
                while (pool.Value.TryDequeue(out poolItem))
                {
                    poolItem.Dispose(true);
                }
            }
            Pools.Clear();
            foreach (var semaphoreSlim in Semaphores)
            {
                if (semaphoreSlim.Value.CurrentCount > 0)
                {
                    try
                    {
                        semaphoreSlim.Value.Release(semaphoreSlim.Value.CurrentCount);
                    }
                    catch 
                    {
                    }
                }
                try
                {
                    semaphoreSlim.Value.Dispose();
                }
                catch
                {
                }
            }
            Semaphores.Clear();
            PoolInitialized = false;
            PoolSuspended = false;
        }
    }
}