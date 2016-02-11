//
// PerRequestProvider.cs
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

namespace Stardust.Nucleus.ContextProviders
{
    /// <summary>
    /// The default implementation for the PerRequest <see cref="Scope"/> provider
    /// </summary>
    public class PerRequestProvider : ScopeProviderBase,IExtendedScopeProvider
    {
        /// <summary>
        /// adds an instance to the scope
        /// </summary>
        /// <param name="type">the type of object to add to the scope</param>
        /// <param name="toInstance">An instance or an instance of a derived class of the given <paramref name="type"/></param>
        protected override void DoBind(Type type, object toInstance)
        {
        }

        public override IExtendedScopeProvider BeginExtendedScope()
        {
            return this;
        }

        public override bool TryBeginExtendedScope(out IExtendedScopeProvider childScope)
        {
            childScope = this;
            return true;
        }

        public override IScopeProvider EndScope(IExtendedScopeProvider scope)
        {
            return this;
        }

        public override bool TryEndScope(IExtendedScopeProvider scope, out IScopeProvider parentScope)
        {
            parentScope = this;
            return true;
        }


        /// <summary>
        /// removes the binding to the type if any.
        /// </summary>
        /// <param name="type">The type to remove from the scope</param>
        public override void InvalidateBinding(System.Type type)
        {
        }

        /// <summary>
        /// Invalidates all bindings within the scope
        /// </summary>
        public override void KillEmAll()
        {
        }


        /// <summary>
        /// Resolves the type from the scope. 
        /// </summary>
        /// <param name="type"></param>
        /// <returns>an instance of the given <paramref name="type"/> or null if unbound</returns>
        protected override object DoResolve(System.Type type)
        {
            return null;
        }

        void IDisposable.Dispose()
        {
            
        }

        public string ScopeId
        {
            get { return Guid.NewGuid().ToString(); }
        }

        public IScopeProvider EndScope()
        {
            return this;
        }

        public IExtendedScopeProvider EndCurrentAndBeginNew()
        {
            return this;
        }
    }
}