//
// bindcontext.cs
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
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;

namespace Stardust.Nucleus.Internals
{
    internal class BindContext : IBindContext
    {
        protected Type[] GenericParameters;
        protected Type ConcreteType;
        protected bool IsUnboundGeneric;
        private IConfigurationKernel ConfigurationKernel;

        internal BindContext(IConfigurationKernel configurationKernel, Type type, bool useUnboundGeneric = false)
            : this(configurationKernel, type, useUnboundGeneric, new Type[] { })
        {

        }

        internal BindContext(IConfigurationKernel configurationKernel, Type type, bool useUnboundGeneric, params Type[] genericParameters)
        {
            ConfigurationKernel = configurationKernel;
            if (useUnboundGeneric)
                ConcreteType = type.GetGenericTypeDefinition();
            else
                ConcreteType = type;
            IsUnboundGeneric = useUnboundGeneric;
            GenericParameters = genericParameters;
            if (ConcreteType.ContainsGenericParameters && !useUnboundGeneric)
                ConcreteType = ConcreteType.MakeGenericType(GenericParameters);
        }

        /// <summary>
        /// Binds an implementation to its base class or interface
        /// </summary>
        public IScopeContext To(Type type, string identifier = "default")
        {
            var existingBinding = ConfigurationKernel.Resolve(ConcreteType, identifier, true);
            if (existingBinding.IsInstance() && !existingBinding.IsNull) return existingBinding;
            existingBinding = type.ConvertToContext();
            ConfigurationKernel.Bind(ConcreteType, existingBinding, identifier);
            return existingBinding;
        }

        public IScopeContext To(Type type, Enum identifier)
        {
            return To(type, identifier.ToString());
        }


    }

    internal sealed class BindContext<T> : BindContext, IBindContext<T>
    {
        internal BindContext(IConfigurationKernel configurationKernel)
            : base(configurationKernel, typeof(T))
        {
        }

        internal BindContext(IConfigurationKernel configurationKernel, params Type[] genericParameters)
            : base(configurationKernel, typeof(T), false, genericParameters)
        {
        }

        /// <summary>
        /// Binds an implementation to its base class or interface
        /// </summary>
        public IScopeContext To<TImplementation>() where TImplementation : T
        {
            var attrib = typeof(TImplementation).GetAttribute<ImplementationKeyAttribute>();
            if (attrib != null) return To<TImplementation>(attrib.Name);
            return To<TImplementation>(TypeLocatorNames.DefaultName);
        }

        /// <summary>
        /// Binds an implementation to its base class or interface
        /// </summary>
        public IScopeContext To<TImplementation>(string identifier) where TImplementation : T
        {
            return base.To(typeof(TImplementation), identifier);
        }

        /// <summary>
        /// Binds an instace of an implementation to its base class or interface
        /// </summary>
        public IScopeContext ToInstance(T instance, string identifier = TypeLocatorNames.DefaultName)
        {
            var ctx= (ScopeContext)base.To(instance.GetType(), identifier);
            ctx.CreatorMethod = () => instance;
            return ctx;
        }

        /// <summary>
        /// Binds an implementation to its base class or interface
        /// </summary>
        public IScopeContext To<TImplementation>(Enum identifier) where TImplementation : T
        {
            return To<TImplementation>(identifier.ToString());
        }

        /// <summary>
        /// Binds the service to it self
        /// </summary>
        public IScopeContext ToSelf()
        {
            return To<T>();
        }

        /// <summary>
        /// Binds the service to it self
        /// </summary>
        public IScopeContext ToSelf(string identifier)
        {
            return To<T>(identifier);
        }

        /// <summary>
        /// Binds the service to it self
        /// </summary>
        public IScopeContext ToSelf(Enum identifier)
        {
            return To<T>(identifier);
        }

        public IScopeContext ToConstructor(Func<object> creator)
        {
            return ToConstructor(creator, TypeLocatorNames.DefaultName);
        }

        public IScopeContext ToConstructor(Func<object> creator, string identifier)
        {
            var scope = (ScopeContext)To(creator().GetType(), identifier);
            scope.CreatorMethod = creator;
            return scope;
        }
    }
}