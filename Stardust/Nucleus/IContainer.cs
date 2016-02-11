//
// icontainer.cs
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
using Stardust.Nucleus.ContextProviders;

namespace Stardust.Nucleus
{
    /// <summary>
    /// This is an OLM container that supports one provider foreach <see cref="Scope"/> type.  
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// Binds an instance of a type to the given <paramref name="scope"/>
        /// </summary>
        /// <param name="type">the type of object to bind to the scope</param>
        /// <param name="toInstance">The instance to bind to the scope</param>
        /// <param name="scope">The Scope to bind the instance to</param>
        /// <remarks>Note that the <param name="toInstance"/> may be a derived type of <param name="type"/></remarks>
        void Bind(Type type, object toInstance, Scope scope);


        /// <summary>
        /// Removes a type from the given <paramref name="scope"/>.
        /// </summary>
        /// <param name="type">The type to remove from the <paramref name="scope"/></param>
        /// <param name="scope">The scope to remove from.</param>
        void InvalidateBinding(Type type, Scope scope);

        /// <summary>
        /// Do not call this from production code. Only to be used by unit tests to reset any cashed instances. Spessialy usefull if you use page or session <see cref="Scope"/>.
        /// </summary>
        void KillAllInstances();

        /// <summary>
        /// Resolves a type with in the <see cref="Scope"/> and returns an instance of that
        /// type if existing, otherwise the instance from 
        /// <paramref name="constructor"/> that is also bound to the <param name="scope"/>.
        /// </summary>
        /// <param name="type">The type to get an instance of.</param>
        /// <param name="scope">the <see cref="Scope"/> to resolve from.</param>
        /// <param name="constructor">an external function that creates a new
        /// instance of the type. This may be database lookup or any other
        /// method that returns an object of type T</param>
        /// <returns></returns>
        object Resolve(Type type, Scope scope, Func<object> constructor);

        /// <summary>
        /// Resolves a type with in the <see cref="Scope"/> and returns an instance of that
        /// type if existing, otherwise null.
        /// </summary>
        /// <param name="type">The type to get an instance of.</param>
        /// <param name="scope">the <see cref="Scope"/> to resolve from.</param>
        object Resolve(Type type, Scope scope);

        IScopeProvider GetProvider(Scope scope);
        /// <summary>
        /// Creates a new OLM sub scope
        /// </summary>
        /// <param name="scopeToExtend"></param>
        /// <returns></returns> 
        IExtendedScopeProvider ExtendScope(Scope scopeToExtend);

    }
}