//
// extensions.cs
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
    public static class Extensions
    {
        private const string ImplementationReferenceCannotBeNull = "Implementation reference cannot be null";
        private const string ImplementationReferenceNameCannotBeNullOrEmpty = "Implementation reference name cannot be null or empty";

        internal static void Validate(this ObjectInitializer implementationReference)
        {
            if (implementationReference.IsNull())
                throw new ModuleCreatorException(ImplementationReferenceCannotBeNull);
            if (implementationReference.Name.IsNullOrEmpty())
                throw new ModuleCreatorException(ImplementationReferenceNameCannotBeNullOrEmpty);
        }

        public static bool Implements(this Type self, Type type)
        {
            if (type.IsInterface)
                return self.GetInterface(type.Name).IsInstance();
            return self.IsSubclassOf(type);
        }

        public static bool Implements<T>(this Type self)
        {
            return self.Implements(typeof(T));
        }

        public static bool Implements(this object self, Type type)
        {
            return !self.IsNull() && self.GetType().Implements(type);
        }

        public static bool Implements<T>(this object self)
        {
            return self.Implements(typeof(T));
        }
    }
}
