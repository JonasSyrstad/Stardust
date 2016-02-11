//
// sessionprovider.cs
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

using System.Diagnostics.CodeAnalysis;
using System.Web;
using Stardust.Particles;

namespace Stardust.Nucleus.ContextProviders
{
    /// <summary>
    /// The default implementation for the Session <see cref="Scope"/> provider
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SessionProvider : SingletonProvider
    {
        /// <summary>
        /// adds an instance to the scope
        /// </summary>
        /// <param name="type">the type of object to add to the scope</param>
        /// <param name="toInstance">An instance or an instance of a derived class of the given <paramref name="type"/></param>
        protected override void DoBind(System.Type type, object toInstance)
        {
            if (HttpContext.Current.IsInstance())
            {
                HttpContext.Current.Session.Add(type.FullName, toInstance);
            }
            else
                base.Bind(type, toInstance);
        }

        /// <summary>
        /// removes the binding to the type if any.
        /// </summary>
        /// <param name="type">The type to remove from the scope</param>
        public override void InvalidateBinding(System.Type type)
        {
            if (HttpContext.Current.IsInstance() && ChildScope.IsNull())
                HttpContext.Current.Session[type.FullName] = null;
            else
                base.InvalidateBinding(type);
        }

        /// <summary>
        /// Invalidates all bindings within the scope
        /// </summary>
        public override void KillEmAll()
        {
            if (HttpContext.Current.IsInstance())
                HttpContext.Current.Items.Clear();
            else
                base.KillEmAll();
        }

        /// <summary>
        /// Resolves the type from the scope. 
        /// </summary>
        /// <param name="type"></param>
        /// <returns>an instance of the given <paramref name="type"/> or null if unbound</returns>
        protected override object DoResolve(System.Type type)
        {
            if (HttpContext.Current.IsInstance())
                return HttpContext.Current.Session[type.FullName];
            return base.Resolve(type);
        }
    }
}