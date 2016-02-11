//
// implementationdefinition.cs
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
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Stardust.Particles;

namespace Stardust.Nucleus.Configuration
{
    [ExcludeFromCodeCoverage]
    public sealed class ImplementationDefinition : ConfigurationElement
    {
        public ImplementationDefinition()
        {
        }

        public ImplementationDefinition(string implementationKey)
        {
            ImplementationKey = implementationKey;
        }

        public ImplementationDefinition(string implementationKey, Type implementationType)
        {
            ImplementationKey = implementationKey;
            type = implementationType;
        }


        [ConfigurationProperty("implementationKey", IsRequired = false, IsKey = true, DefaultValue = "default")]
        public string ImplementationKey
        {
            get { return (string)this["implementationKey"]; }
            set { this["implementationKey"] = value; }
        }

        /// <summary>
        /// The location of the assembly
        /// </summary>
        [ConfigurationProperty("assemblyPath", IsRequired = false)]
        public string AssemblyPath
        {
            get { return (string)this["assemblyPath"]; }
            set { this["assemblyPath"] = value; }
        }

        /// <summary>
        /// The fully qualified type name
        /// </summary>
        [ConfigurationProperty("type", IsRequired = false)]
        public Type type
        {
            get { return (Type)this["type"]; }
            set { this["type"] = value; }
        }
        /// <summary>
        /// The activation scope for instances of this type
        /// </summary>
        [ConfigurationProperty("scope", IsRequired = false, DefaultValue = "Context")]
        public Scope Scope
        {
            get { return (Scope)this["scope"]; }
            set { this["scope"] = value; }
        }


        /// <summary>
        /// The full name of the type, not the qualified type name as this is resolved in combination with the assembly path
        /// </summary>
        [ConfigurationProperty("typeFullName", IsRequired = false)]
        public string TypeFullName
        {
            get { return (string)this["typeFullName"]; }
            set { this["typeFullName"] = value; }
        }

        protected override void PostDeserialize()
        {
            base.PostDeserialize();
            if (TypeFullName.ContainsCharacters() != AssemblyPath.ContainsCharacters())
                throw new ConfigurationErrorsException(string.Format("Invalid configuration [{0}]: both TypeFullName and AssemblyPath must have value or none of them.", ImplementationKey));
            if (type != null && TypeFullName.ContainsCharacters())
                throw new ConfigurationErrorsException(string.Format("Invalid configuration [{0}]: You can only specify type or TypeFullName/AssemblyPath not both", ImplementationKey));
            
        }
    }

}