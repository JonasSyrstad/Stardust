//
// StateStorageTask.cs
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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Stardust.Interstellar.Tasks
{
    internal class StateStorageTask : AbstractRuntimeTask, IStateStorageTask
    {
        public StateStorageTask(IRuntime runtime)
            : base(runtime)
        {
        }

        private readonly ConcurrentDictionary<string, StateStorageItem> Items = new ConcurrentDictionary<string, StateStorageItem>();

        public void AddStorageItem(StateStorageItem item)
        {
            string error;
            if (!TryAddStorageItem(item, out error)) throw new InvalidDataException(error);
        }

        public bool TryAddStorageItem(StateStorageItem item, out string errorMessage)
        {
            if (!item.IsValid())
            {
                errorMessage = string.Format("Invalid storage item: {0}", item.LogString());
                return false;
            }
            if (!Items.TryAdd(item.ItemName, item))
            {
                errorMessage = string.Format("An item with name '{0}' already exists", item.ItemName);
                return false;
            }
            errorMessage = null;
            return true;
        }

        public bool TryAddStorageItem(StateStorageItem item)
        {
            string error;
            return TryAddStorageItem(item, out error);
        }


        public bool TryAddStorageItem<T>(T item)
        {
            return TryAddStorageItem(item, typeof(T).Name);
        }

        public bool TryAddStorageItem<T>(T item, string key)
        {
            return
                TryAddStorageItem(new StateStorageItem
                {
                    ItemName = key,
                    RuntimeItemName = typeof(T).FullName,
                    Value = item
                });
        }

        public IEnumerable<StateStorageItem> GetStorageItems()
        {
            return Items.Values;
        }

        public bool TryGetItem(string key, out StateStorageItem item)
        {
            return Items.TryGetValue(key, out item);
        }

        public bool TryGetItem<T>(string key, out T item)
        {
            StateStorageItem storageItem;
            if (TryGetItem(key, out storageItem))
            {
                if (storageItem.Value is T)
                {
                    item = (T)storageItem.Value;
                    return true;
                }
            }
            item = default(T);
            return false;
        }

        public bool TryGetItem<T>(out T item)
        {
            return TryGetItem(typeof(T).Name, out item);
        }

        public StateStorageItem this[string key]
        {
            get
            {
                StateStorageItem item;
                if (TryGetItem(key, out item)) return item;
                throw new IndexOutOfRangeException(string.Format("Key '{0}'is not found in the container", key));
            }
        }

        public T GetItem<T>(string key)
        {
            T item;
            if (!TryGetItem(key, out item)) throw new IndexOutOfRangeException(string.Format("Key '{0}'is not found in the container", key));
            return item;
        }

        public T GetItem<T>()
        {
            return GetItem<T>(typeof (T).Name);
        }

        public IEnumerator<StateStorageItem> GetEnumerator()
        {
            return Items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
