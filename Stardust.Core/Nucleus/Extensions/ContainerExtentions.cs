//
// ContainerExtentions.cs
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
using Stardust.Particles;

namespace Stardust.Nucleus.Extensions
{
    /// <summary>
    /// Contains generic versions of the bind and resolve methods in IContainer. 
    /// </summary>
    /// <remarks>Note that it uses the type of T to bind and resolve, not the actual type of the instance.</remarks>
    public static class ContainerExtensions
    {
        /// <summary>
        /// Binds an instance of the type T to the given scope.
        /// </summary>
        /// <typeparam name="T">The type to cache in the given scope</typeparam>
        /// <param name="container">The OLM infrastructure</param>
        /// <param name="instance">An instance of type T to bind to the scope</param>
        /// <param name="scope">The OLM scope to bind to</param>
        public static void Bind<T>(this IContainer container, T instance, Scope scope)
        {
            container.Bind(typeof(T),instance,scope);
        }

        /// <summary>
        /// finds an instance of T in the given scope, if not present the default value of T is returned
        /// </summary>
        /// <typeparam name="T">The type to resolve in the given scope</typeparam>
        /// <param name="container">The OLM infrastructure</param>
        /// <param name="scope">The OLM scope to resolve from</param>
        /// <returns>the cached instance of T or default(T)</returns>
        public static T Resolve<T>(this IContainer container, Scope scope)
        {
            var instance = container.Resolve(typeof (T), scope);
            return instance is T ? (T) instance : default(T);
        }

        /// <summary>
        /// finds an instance of T in the given scope, if not present the default value of T is returned
        /// </summary>
        /// <typeparam name="T">The type to resolve in the given scope</typeparam>
        /// <param name="container">The OLM infrastructure</param>
        /// <param name="scope">The OLM scope to resolve from</param>
        /// <param name="creationMethod">A method that returnes an instance of T</param>
        /// <param name="initialization">An external method run on the newly created instance before it is added to the scope</param>
        /// <returns>the cached instance of T or the instance created through the creationMethod</returns>
        public static T Resolve<T>(this IContainer container, Scope scope, Func<T> creationMethod ,Action<T> initialization)
        {
            var instance = container.Resolve<T>(scope);
            if (instance.IsInstance() || creationMethod.IsNull()) return instance;
            instance = creationMethod();
            if (initialization.IsInstance())
                initialization(instance);
            container.Bind(instance,scope);
            return instance;
        }

        /// <summary>
        /// finds an instance of T in the given scope, if not present the default value of T is returned
        /// </summary>
        /// <typeparam name="T">The type to resolve in the given scope</typeparam>
        /// <param name="container">The OLM infrastructure</param>
        /// <param name="scope">The OLM scope to resolve from</param>
        /// <param name="creationMethod">A method that returnes an instance of T</param>
        /// <returns>the cached instance of T or the instance created through the creationMethod</returns>
        public static T Resolve<T>(this IContainer container, Scope scope, Func<T> creationMethod)
        {
            return container.Resolve(scope, creationMethod,null);
        }

        /// <summary>
        /// Moves an instance of an object from one OLM scope to another
        /// </summary>
        /// <typeparam name="T">The type of instance to promote</typeparam>
        /// <param name="container">The OLM infrastructure</param>
        /// <param name="from">Source scope</param>
        /// <param name="to">Destination scope</param>
        public static void Promote<T>(this IContainer container, Scope from, Scope to)
        {
            typeof(T).PromoteObject(@from, to);
        }
    }
}