//
// IStateStorageTask.cs
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

using System.Collections.Generic;
using System.IO;

namespace Stardust.Interstellar.Tasks
{
    /// <summary>
    /// Act as a storage compartment during the <see cref="IRuntime"/> lifetime. Used to share common data between task involved in the execution. This is thread safe.
    /// </summary>

    public interface IStateStorageTask : IRuntimeTask, IEnumerable<StateStorageItem>
    {
        /// <summary>
        /// adds an item to the store
        /// </summary>
        /// <param name="item">the item to add</param>
        /// <exception cref="InvalidDataException"></exception>
        /// <remarks>The <paramref name="item"/> is retrieved by using the ItemName as key</remarks>
        void AddStorageItem(StateStorageItem item);

        /// <summary>
        /// Adds an item to the store if item is valid and has not been added before
        /// </summary>
        /// <param name="item">the item to store</param>
        /// <param name="errorMessage">The error message if it fails/param>
        /// <returns>true if the item is added to the store</returns>
        /// <remarks>The <paramref name="item"/> is retrieved by using the ItemName as key</remarks>
        bool TryAddStorageItem(StateStorageItem item, out string errorMessage);

        /// <summary>
        /// adds an item to the store if item is valid and has not been added before
        /// </summary>
        /// <param name="item">the item to store</param>
        /// <returns>true if the item is added to the store</returns>
        ///  <remarks>The <paramref name="item"/> is retrieved by using the ItemName as key</remarks>
        bool TryAddStorageItem(StateStorageItem item);

        /// <summary>
        /// Adds an item to the store if item is valid and has not been added before
        /// </summary>
        /// <param name="item">the item to store, the class name is used as key</param>
        /// <typeparam name="T">The type of item to store</typeparam>
        /// <returns>true if the item is added to the store</returns>
        bool TryAddStorageItem<T>(T item);



        /// <summary>
        /// Adds an item to the store if item is valid and has not been added before
        /// </summary>
        /// <param name="item">the item to store</param>
        /// <param name="key">the lookup key to use for getting the element later</param>
        /// <typeparam name="T">The type of item to store</typeparam>
        /// <returns>true if the item is added to the store</returns>
        bool TryAddStorageItem<T>(T item, string key);

        /// <summary>
        /// Returns all storage items from the container
        /// </summary>
        /// <returns>an <see cref="T:System.Collections.Generic.IEnumerable'1"/> of <see cref="StateStorageItem"/></returns>
        IEnumerable<StateStorageItem> GetStorageItems();


        /// <summary>
        /// gets an item from the store
        /// </summary>
        /// <param name="key">the key to use for lookup</param>
        /// <param name="item">the stored item</param>
        /// <returns></returns>
        bool TryGetItem(string key, out StateStorageItem item);

        /// <summary>
        /// gets an item from the store
        /// </summary>
        /// <param name="key">the key to use for lookup</param>
        /// <param name="item">the stored item</param>
        /// <returns></returns>
        bool TryGetItem<T>(string key, out T item);

        /// <summary>
        /// gets an item from the store, uses the type name as key
        /// </summary>
        /// <param name="item">the stored item</param>
        /// <returns></returns>
        bool TryGetItem<T>(out T item);

        /// <summary>
        /// Gets an item from the store
        /// </summary>
        /// <param name="key">The lookup key</param>
        /// <returns>an instance </returns>
        StateStorageItem this[string key] { get; }

        /// <summary>
        /// Gets an item from the store
        /// </summary>
        /// <param name="key">The lookup key</param>
        /// <returns>an instance </returns>
        T GetItem<T>(string key);

        /// <summary>
        /// Gets an item from the store
        /// </summary>
        /// <returns>an instance </returns>
        T GetItem<T>();


    }
}