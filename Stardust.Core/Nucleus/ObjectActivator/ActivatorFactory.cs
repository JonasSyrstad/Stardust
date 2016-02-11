//
// activatorfactory.cs
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
using Stardust.Nucleus.Configuration;

namespace Stardust.Nucleus.ObjectActivator
{
    /// <summary>
    /// Factory class for object activators
    /// </summary>
    public static class ActivatorFactory
    {
        private static readonly IActivator DefaultActivator = new DefaultActivator();
        private static IActivator AlternateActivator;
        private static readonly object LockObject = new object();
        private static bool IsInitialized;

        public static void SetActivator<T>() where T : IActivator, new()
        {
            IsInitialized = true;
            lock (LockObject)
                AlternateActivator = new T();
        }

        /// <summary>
        /// Activates the full DI activator implementation. This activator resolves and activates Properties and Filds of the parent object before returning to the client.
        /// To tell the activator to instantiate properties and feilds add the Bound attribute to the members.
        /// </summary>
        /// <remarks>You can turn this feature on by adding ModuleCreators UseBoundActivator="true" attribute in your config file.</remarks>
        public static void SetBoundActivator()
        {
            SetActivator<BoundActivator>();
        }

        /// <summary>
        /// Use this to revert to the default activator.
        /// </summary>
        public static void ResetActivator()
        {
            lock (LockObject)
                AlternateActivator = null;
        }

        public static IActivator Activator
        {
            get
            {
                if(!IsInitialized)
                {
                    if (ConfigurationHelper.UseBoundActivator())
                        SetBoundActivator();
                    IsInitialized=true;
                }
                if (AlternateActivator != null)
                    return AlternateActivator;
                return DefaultActivator;
            }
        }

        public static Type MakeConcreteType(this Type self, params Type[] genericParameters)
        {
            return self.IsGenericTypeDefinition ? self.MakeGenericType(genericParameters) : self;
        }
    }
}