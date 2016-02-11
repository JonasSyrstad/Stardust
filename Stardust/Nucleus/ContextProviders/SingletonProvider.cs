//
// singletonprovider.cs
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
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;

namespace Stardust.Nucleus.ContextProviders
{
    /// <summary>
    /// The default implementation for the Singleton <see cref="Scope"/> provider
    /// </summary>
    public class SingletonProvider : ScopeProviderBase
    {
        private static ConcurrentDictionary<Type, object> Singletons;

        public SingletonProvider()
        {
            Singletons = new ConcurrentDictionary<Type, object>();
        }

        protected override IExtendedScopeProvider ChildScope { get; set; }


        protected override void DoBind(Type type, object toInstance)
        {
            if (Singletons.ContainsKey(type)) return;
            if (Singletons.ContainsKey(type)) return;
            Singletons.TryAdd(type, toInstance);
        }

        public override void InvalidateBinding(Type type)
        {
            if (ItemDoesNotExist(type)) return;
            object item;
            Singletons.TryRemove(type, out item);
        }

        public override void KillEmAll()
        {
            var kernel = DoResolve(typeof(IConfigurationKernel));
            var child = ChildScope;
            Singletons.Clear();
            if (kernel.IsInstance())
                Bind(typeof(IConfigurationKernel), kernel);
            if (child.IsInstance())
                Bind(typeof(IExtendedScopeProvider), child);
        }

        protected override object DoResolve(Type type)
        {
            object value;
            Singletons.TryGetValue(type, out value);
            return value;
        }

        private static bool ItemDoesNotExist(Type type)
        {
            return !Singletons.ContainsKey(type);
        }

        public override bool TryEndScope(IExtendedScopeProvider scope, out IScopeProvider parentScope)
        {
            if (!ValidateScope(scope, out parentScope)) return false;
            ChildScope.Dispose();
            parentScope = this;
            ChildScope = null;
            return true;
        }

        public override IScopeProvider EndScope(IExtendedScopeProvider scope)
        {
            ValidateEndScopeWhitException(scope);
            ChildScope.Dispose();
            ChildScope = null;
            return this;
        }
    }
}