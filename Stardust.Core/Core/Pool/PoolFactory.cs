//
// poolfactory.cs
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
using System.Threading.Tasks;
using Stardust.Particles;

namespace Stardust.Core.Pool
{
    public static class PoolFactory
    {
        private static readonly ConcurrentDictionary<Type, PoolContainerBase> Pools = new ConcurrentDictionary<Type, PoolContainerBase>();

        public static void DisposePool<T>(bool reinitializePool = true) where T : PoolableBase, new()
        {
            GetPool<T>().DisposeAll(reinitializePool);
        }

        public static Task DisposePoolAsync<T>(bool reinitializePool = true) where T : PoolableBase, new()
        {
            return GetPool<T>().DisposeAllAsync(reinitializePool);
        }

        public static T Create<T>() where T : PoolableBase, new()
        {
            return GetPool<T>().GetItemFromPool();
        }

        public static Task<T> CreateAsync<T>() where T : PoolableBase, new()
        {
            return GetPool<T>().GetItemFromPoolAsync();
        }

        private static PoolContainer<T> GetPool<T>() where T : PoolableBase, new()
        {
            if (Pools.ContainsKey(typeof(T)))
            {
                return (PoolContainer<T>)Pools[typeof(T)];
            }
            throw new ArgumentOutOfRangeException("Pool does not exist");
        }

        private static void CreateNewPool<T>(int poolSize, Action<T> initializationCode) where T : PoolableBase, new()
        {
            CreateBasicPool(poolSize, initializationCode);
        }

        private static void CreatePoolWithConnectionstrings<T>(int poolSize, Action<T> initializationCode) where T : ConnectionStringPoolableBase, new()
        {
            lock (Pools)
            {
                if (Pools.ContainsKey(typeof(T)))
                {
                    PoolContainerBase oldContainer;
                    if (!Pools.TryRemove(typeof(T), out oldContainer))
                    {
                        throw new InvalidOperationException("Unable to remove old pool");
                    }
                    oldContainer.DisposeAll(false);
                }
                var newPool = new ConnectionStringPoolContainer<T>(poolSize, initializationCode);
                Pools.AddOrIgnore(typeof(T), newPool);
            }
        }

        private static void CreateBasicPool<T>(int poolSize, Action<T> initializationCode) where T : PoolableBase, new()
        {
            lock (Pools)
            {
                if (Pools.ContainsKey(typeof(T)))
                {
                    PoolContainerBase oldContainer;
                    if (!Pools.TryRemove(typeof(T), out oldContainer))
                    {
                        throw new InvalidOperationException("Unable to remove old pool");
                    }
                    oldContainer.DisposeAll(false);
                }
                var newPool = new PoolContainer<T>(poolSize, initializationCode);
                Pools.AddOrIgnore(typeof(T), newPool);
            }
        }

        public static void InitializePool<T>(int poolSize, Action<T> initializationCode)
            where T : PoolableBase, new()
        {
            CreateNewPool(poolSize, initializationCode);
        }

        public static void InitializeNamedPool<T>(int poolSize, Action<T> initializationCode)
            where T : ConnectionStringPoolableBase, new()
        {
            CreatePoolWithConnectionstrings(poolSize, initializationCode);
        }

        public static PoolStatus GetPoolStatus<T>() where T : PoolableBase, new()
        {
            var pool = GetPool<T>();
            return pool.GetStatus();
        }

        public static T Create<T>(string connectionString) where T : ConnectionStringPoolableBase, new()
        {
            var pool = GetNamedPool<T>();
            return pool.GetItemFromPool(connectionString);
        }

        public static async Task<T> CreateAsync<T>(string connectionString) where T : ConnectionStringPoolableBase, new()
        {
            var pool = GetNamedPool<T>();
            return await pool.GetItemFromPoolAsync(connectionString);
        }

        private static ConnectionStringPoolContainer<T> GetNamedPool<T>() where T : ConnectionStringPoolableBase, new()
        {
            if (Pools.ContainsKey(typeof(T)))
            {
                return (ConnectionStringPoolContainer<T>)Pools[typeof(T)];
            }
            throw new ArgumentOutOfRangeException("Pool does not exist");
        }

        public static void DisposeNamedPool<T>() where T : ConnectionStringPoolableBase, new()
        {
            var pool = GetNamedPool<T>();
            pool.DisposeAll(false);
        }
    }
}