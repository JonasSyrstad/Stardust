//
// scopecontext.cs
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
using JetBrains.Annotations;
using Stardust.Particles;

namespace Stardust.Nucleus.Internals
{
    internal interface IScopeContextInternal : IScopeContext
    {
        Func<object> CreatorMethod { get;  }
        Type BoundType { get; }

        ScopeContext SetScope(Scope scope);
    }

    internal sealed class ScopeContext : IScopeContextInternal
    {

        private static Scope? DefaultScope;
        private Scope? PrivateActivationScope;

        internal static void SetDefaultActivationScope(Scope activationScope)
        {
            DefaultScope = activationScope;
        }

        internal static Scope GetDefaultScope()
        {
            if (DefaultScope.HasValue) return DefaultScope.Value;
            return Scope.Context;
        }
        internal ScopeContext([NotNull] Type boundType, string implementationKey = null)
        {
            BoundType = boundType;
            ImplementationKey = implementationKey;
        }

        internal ScopeContext(Func<object> creatorMethod)
        {
            CreatorMethod = creatorMethod;
            BoundType = creatorMethod().GetType();
        }

        private ScopeContext()
        {
            IsNull = true;
        }

        public Func<object> CreatorMethod { get; set; }

        public string ImplementationKey { get; private set; }

        public Type BoundType { get; private set; }

        /// <summary>
        /// Contains the current activation scope for the registration
        /// </summary>
        public Scope? ActivationScope
        {
            get
            {
                return PrivateActivationScope.HasValue ? PrivateActivationScope : DefaultScope;
            }
            private set { PrivateActivationScope = value; }
        }

        /// <summary>
        /// Set to true if the client code is allowed to provide it's own scope
        /// </summary>
        public bool AllowOverride { get; set; }

        public IScopeContext SetTransientScope()
        {
            return SetScope(Scope.PerRequest);
        }

        public IScopeContext SetRequestResponseScope()
        {
            return SetScope(Scope.Context);
        }

        public IScopeContext SetSessionScope()
        {
            return SetScope(Scope.Session);
        }

        public IScopeContext SetThreadScope()
        {
            return SetScope(Scope.Thread);
        }

        public IScopeContext SetSingletonScope()
        {
            return SetScope(Scope.Singleton);
        }

        private static string GetDisableOverrideSettings
        {
            get
            {
                var value = ConfigurationManagerHelper.GetValueOnKey("stardust.DisableOverride");
                return value.ContainsCharacters() ? value : "";
            }
        }

        public void SetAllowOverride()
        {
            AllowOverride = true;
        }

        public void DisableOverride()
        {
            AllowOverride = false;
        }

        public T GetAttribute<T>() where T : Attribute
        {
            return BoundType.GetAttribute<T>();
        }


        public ScopeContext SetScope(Scope scope)
        {
            ActivationScope = scope;
            AllowOverride = GetDisableOverrideSettings.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            return this;
        }

        public static ScopeContext NullInstance()
        {
            return new ScopeContext();
        }

        public bool IsNull { get; private set; }
    }
}