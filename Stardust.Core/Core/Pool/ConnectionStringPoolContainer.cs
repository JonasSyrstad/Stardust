//
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
using Stardust.Interstellar;
using Stardust.Particles;

namespace Stardust.Core.Pool
{
    internal class ConnectionStringPoolContainer<T> : PoolContainerBase where T : ConnectionStringPoolableBase, new()
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<ConnectionStringPoolableBase>> Pools = new ConcurrentDictionary<string, ConcurrentQueue<ConnectionStringPoolableBase>>();
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
            var pool = Pools[item.PoolName];
            pool.Enqueue(item);
            item.Used = false;
            InUse--;
        }

        public Task<T> GetItemFromPoolAsync(string connectionString)
        {
            return RuntimeFactory.Run(() => GetItemFromPool(connectionString));
        }

        public T GetItemFromPool(string connectionString)
        {
            if (PoolSuspended)
            {
                throw new InvalidAsynchronousStateException("Pool is suspended for dispose operation");
            }
            ConnectionStringPoolableBase item;
            if (!Pools.ContainsKey(GetPoolName(connectionString)))
                InitializePool(connectionString);
            var pool = Pools[GetPoolName(connectionString)];
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
            return (T)item;
        }

        private void InitializePool(string connectionString)
        {
            lock (Pools)
            {
                if (Pools.ContainsKey(GetPoolName(connectionString))) return;
                var pool = new ConcurrentQueue<ConnectionStringPoolableBase>();
                if (Pools.ContainsKey(GetPoolName(connectionString))) return;
                CreatePoolItems(pool, connectionString);
                PoolInitialized = true;
                Pools.AddOrUpdate(GetPoolName(connectionString), pool);
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
            PoolInitialized = false;
            PoolSuspended = false;
        }
    }
}