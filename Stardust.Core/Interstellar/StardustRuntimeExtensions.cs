//
// StardustRuntimeExtensions.cs
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
using System.Dynamic;

namespace Stardust.Interstellar
{
    public static class StardustRuntimeExtensions
    {
        /// <summary>
        /// Grabs a config value for the current environment
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetProperty(this IRuntime runtime,string name)
        {
            return runtime.Context.GetEnvironmentConfiguration().GetConfigParameter(name);
        }

        /// <summary>
        /// Returns true/false value for a given envirionment key, if the config value is not true the it returns false.
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetPropertySwitch(this IRuntime runtime, string name)
        {
            return runtime.Context.GetEnvironmentConfiguration().GetConfigParameter(name).Equals("true",StringComparison.InvariantCultureIgnoreCase);
        }

        public static string GetSecureProperty(this IRuntime runtime, string name)
        {
            return runtime.Context.GetEnvironmentConfiguration().GetSecureConfigParameter(name);
        }

        /// <summary>
        /// Returns a dynamic object that allows you to do the following:
        /// Get a property value by: runtime.GetSettins().Address
        /// Get an encrypted property value by adding Secure to the property name: runtime.GetSettins().SecurePassword
        /// To grab a service host property use (this grabs a host property from the current host.):
        /// runtime.GetSettins().HostAddress or runtime.GetSettins().SecureHostPassword
        /// </summary>
        /// <param name="runtime"></param>
        /// <returns></returns>
        public static dynamic GetSettings(this IRuntime runtime)
        {
            return new RuntimeSettings(runtime);
        }

        /// <summary>
        /// Grabs a config value for the current environment
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetHostProperty(this IRuntime runtime, string name)
        {
            return runtime.Context.GetServiceConfiguration().GetConfigParameter(name);
        }

        /// <summary>
        /// Returns true/false value for a given envirionment key, if the config value is not true the it returns false.
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetHostPropertySwitch(this IRuntime runtime, string name)
        {
            return runtime.Context.GetServiceConfiguration().GetConfigParameter(name).Equals("true", StringComparison.InvariantCultureIgnoreCase);
        }

        public static string GetSecureHostProperty(this IRuntime runtime, string name)
        {
            return runtime.Context.GetServiceConfiguration().GetSecureConfigParameter(name);
        }
    }

    public class RuntimeSettings : DynamicObject
    {
        private readonly IRuntime runtime;

        internal RuntimeSettings(IRuntime runtime)
        {
            this.runtime = runtime;
        }

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)
        /// </returns>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param><param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result"/>.</param>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return binder.Name.Contains("Host") ? 
                GetHostProperty(binder, out result) : 
                GetEnvironmentProperty(binder, out result);
        }

        private bool GetEnvironmentProperty(GetMemberBinder binder, out object result)
        {
            try
            {
                if (binder.Name.StartsWith("Secure"))
                {
                    result = runtime.GetSecureProperty(binder.Name.Replace("Secure", ""));
                    return true;
                }
                result = runtime.GetProperty(binder.Name);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        private bool GetHostProperty(GetMemberBinder binder, out object result)
        {
            try
            {
                if (binder.Name.StartsWith("Secure"))
                {
                    result = runtime.GetSecureHostProperty(binder.Name.Replace("SecureHost", ""));
                    return true;
                }
                result = runtime.GetHostProperty(binder.Name.Replace("Host", ""));
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }
    }
}
