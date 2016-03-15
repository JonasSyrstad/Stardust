//
// BoundAttribute.cs
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

namespace Stardust.Nucleus
{
    [AttributeUsage(AttributeTargets.Class ,AllowMultiple = true)]
    public sealed class ImplementationKeyAttribute : Attribute
    {
        public string Name { get; set; }

        public ImplementationKeyAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Attribute"/> class.
        /// </summary>
        public ImplementationKeyAttribute()
        {
            Name = TypeLocatorNames.DefaultName;
        }
    }

    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field)]
    public sealed class BoundAttribute: Attribute
    {
        public BoundAttribute()
        {
            ScopeContext = Scope.PerRequest;
        }

        public BoundAttribute(string resolverContext)
        {
            ScopeContext = Scope.PerRequest;
            ResolverContext = resolverContext;
        }

        public BoundAttribute(Type boundToType, Scope scope)
        {
            BoundTo = boundToType;
            ScopeContext = scope;
        }

        public BoundAttribute(Type boundToType, Scope scope, string resolverContext)
        {
            BoundTo = boundToType;
            ScopeContext = scope;
            ResolverContext = resolverContext;
        }

        public BoundAttribute( Scope scope)
        {
            ScopeContext = scope;
        }

        public BoundAttribute(Scope scope,string resolverContext)
        {
            ScopeContext = scope;
            ResolverContext = resolverContext;
        }

        public BoundAttribute(Type boundToType)
        {
            ScopeContext = Scope.PerRequest;
            BoundTo = boundToType;
        }

        public Type BoundTo { get; private set; }

        public Scope ScopeContext { get; private set; }

        public string ResolverContext { get; private set; }
    }
}