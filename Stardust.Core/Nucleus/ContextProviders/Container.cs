//
// container.cs
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
using System.Collections.Generic;
using Stardust.Nucleus.ObjectActivator;
using Stardust.Particles;

namespace Stardust.Nucleus.ContextProviders
{
    public class Container : IContainer
    {
        private readonly Dictionary<Scope, IScopeProvider> Providers = new Dictionary<Scope, IScopeProvider>();
        private static readonly object ResolverLock = new object();

        private readonly Type SingletonProvider = typeof(SingletonProvider);
        private readonly Type ContextProvider = typeof(ContextProvider);
        private readonly Type SessionProvider = typeof(SessionProvider);
        private readonly Type PerRequestProvider = typeof(PerRequestProvider);
        private readonly Type ThreadProvider = GetThreadProvider();

        private static Type GetThreadProvider()
        {
            return ConfigurationManagerHelper.GetValueOnKey("stardust.useThreadScope", false) ? typeof(ThreadProvider) : typeof(PerRequestProvider);
        }

        internal static Type ExtendedContextProvider = typeof(ExtendedScopeProvider);

        protected Container()
        {
        }

        public void Bind(Type type, object toInstance, Scope scope)
        {
            GetProvider(scope).Bind(type, toInstance);
        }


        //public virtual void BindScopeProviders()
        //{
        //    lock (ResolverLock)
        //    {
        //        Resolver.Bind<IScopeProvider>().To<PerRequestProvider>(CreateProviderName(Scope.PerRequest));
        //        Resolver.Bind<IScopeProvider>().To<ContextProvider>(CreateProviderName(Scope.Context));
        //        Resolver.Bind<IScopeProvider>().To<SessionProvider>(CreateProviderName(Scope.Session));
        //        Resolver.Bind<IScopeProvider>().To<SingletonProvider>(CreateProviderName(Scope.Singleton));
        //        Resolver.Bind<IScopeProvider>().To<ThreadProvider>(CreateProviderName(Scope.Thread));
        //    }
        //}

        public void InvalidateAllProviders()
        {
            lock (Providers)
                Providers.Clear();
        }

        public void InvalidateBinding(Type type, Scope scope)
        {
            GetProvider(scope).InvalidateBinding(type);
        }

        public void KillAllInstances()
        {
            foreach (var scopeProvider in Providers)
            {
                if (scopeProvider.IsInstance())
                    scopeProvider.Value.KillEmAll();
            }
        }

        public object Resolve(Type type, Scope scope)
        {
            return GetProvider(scope).Resolve(type);
        }

        public object Resolve(Type type, Scope scope, Func<object> constructor)
        {
            try
            {
                var item = Resolve(type, scope);
                if (item.IsNull())
                    item = constructor();
                Bind(type, item, scope);
                return item;
            }
            catch (Exception)
            {
                return constructor();
            }
        }

        private static string CreateProviderName(Scope scope)
        {
            return scope + "Provider";
        }

        private void CreateAndAddProvider(Scope scope)
        {
            lock (Providers)
                if (!Providers.ContainsKey(scope))
                {
                    var provider = (IScopeProvider)ObjectFactory.CreateConstructor(GetProviderType(scope))();
                    if (Providers.ContainsKey(scope)) return;
                    Providers.Add(scope, provider);
                }

        }

        private Type GetProviderType(Scope scope)
        {
            switch (scope)
            {
                case Scope.Singleton:
                    return SingletonProvider;
                case Scope.PerRequest:
                    return PerRequestProvider;
                case Scope.Context:
                    return ContextProvider;
                case Scope.Session:
                    return SessionProvider;
                case Scope.Thread:
                    return ThreadProvider;
                default:
                    throw new ArgumentOutOfRangeException("scope");
            }
        }

        public IScopeProvider GetProvider(Scope scope)
        {
            IScopeProvider value;
            if (!Providers.TryGetValue(scope, out value))
            {
                CreateAndAddProvider(scope);
                return Providers[scope];
            }
            return value;
        }

        public IExtendedScopeProvider ExtendScope(Scope scope)
        {
            return GetProvider(scope).BeginExtendedScope();
        }
    }
}