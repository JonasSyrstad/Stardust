//
// iactivator.cs
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

namespace Stardust.Nucleus
{
    /// <summary>
    /// Creates an instance of T 
    /// </summary>
    public interface IActivator
    {

        /// <summary>
        /// Creates an instance of the <paramref name="type"/> and casts it to <typeparamref name="T"/>
        /// </summary>
        /// <param name="type">a type that implements/inherits from T</param>
        /// <typeparam name="T">The type to return, this mask the subtype if any from the client</typeparam>
        /// <returns>An instance of <paramref name="type"/> as T</returns>
        T Activate<T>(Type type);

        /// <summary>
        /// Creates an instance of the <paramref name="type"/> and casts it to <typeparamref name="T"/>
        /// </summary>
        /// <param name="type">the type to create an instance of. This must be T or a subtype of T</param>
        /// <param name="genericParameters">The generic type parameters to use when construction the concrete type</param>
        /// <param name="args">Any constructor arguments to use when creating the instance</param>
        /// <typeparam name="T">The type to return, this mask the subtype if any from the client</typeparam>
        /// <returns>An instance of <paramref name="type"/> as T</returns>
        T ActivateWithExternalGenerics<T>(Type type, Type[] genericParameters);

        /// <summary>
        /// Creates an instance of the <paramref name="type"/>
        /// </summary>
        /// <param name="type">the type to create an instance of. This must be T or a subtype of T</param>
        /// <param name="genericParameters">The generic type parameters to use when construction the concrete type</param>
        /// <returns>An instance of the given type</returns>
        object ActivateWithExternalGenerics(Type type, Type[] genericParameters);
        
        /// <summary>
        /// Creates an instance of the <paramref name="type"/>
        /// </summary>
        /// <param name="type">the type to create an instance of.</param>
        /// <returns>An instance of the given type</returns>
        object Activate(Type type);
    }
}