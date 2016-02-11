//
// objectinitializer.cs
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
using System.Reflection;
using System.Xml.Serialization;
using Stardust.Particles;

namespace Stardust.Nucleus
{
    [Obsolete("Trying to get rid of this", false)]
    public sealed class ObjectInitializer
    {
        private Type ModuleType1;
        public string Name { get; set; }

        public string FullName { get; set; }

        public string AssemblyName { get; set; }

        public string AssemblyPath { get; set; }

        [XmlIgnore]
        public Type ModuleType
        {
            get
            {
                if (ModuleType1.IsNull())
                    ModuleType1 =GetTypeFromAssembly();
                return ModuleType1;
            }
            set
            {
                ModuleType1 = value;
                ModuleTypeName = value.ToString();
            }
        }

        private Type GetTypeFromAssembly()
        {
            return Assembly.LoadFrom(AssemblyPath.ContainsCharacters() ? AssemblyPath : AssemblyName)
                .GetType(ModuleTypeName);
        }

        public string ModuleTypeName { get; set; }

        public Scope Scope { get; set; }

        public override string ToString()
        {
            if (FullName.ContainsCharacters())
                return FullName;
            return Name;
        }

        public bool IsValid()
        {
            return UseFullNameAndAssembly() 
                || UseNameOnly();
        }

        internal bool UseNameOnly()
        {
            return Name.ContainsCharacters() 
                && (AssemblyName.IsNullOrEmpty() 
                || FullName.IsNullOrEmpty());
        }

        internal bool UseFullNameAndAssembly()
        {
            return AssemblyName.ContainsCharacters()
                && FullName.ContainsCharacters();
        }

        internal bool ShouldLoadFromFile()
        {
            return UseFullNameAndAssembly() 
                && AssemblyPath.ContainsCharacters();
        }

        internal bool IsInvalid()
        {
            return !IsValid();
        }

        public static ObjectInitializer Default
        {
            get { return new ObjectInitializer {Name = "default"}; }
        }
    }
}