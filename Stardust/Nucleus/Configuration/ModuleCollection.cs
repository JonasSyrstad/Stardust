//
// ModuleCollection.cs
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
    [ConfigurationCollection(typeof(MappingElement))]
    public sealed class ModuleCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new MappingElement();
        }

        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            return new MappingElement(elementName);
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((MappingElement)element).BaseModuleName;
        }

        public new string AddElementName
        {
            get { return base.AddElementName; }
            set { base.AddElementName = value; }
        }

        public new string ClearElementName
        {
            get { return base.ClearElementName; }
            set { base.AddElementName = value; }
        }

        public new string RemoveElementName
        {
            get { return base.RemoveElementName; }
        }

        public new int Count
        {
            get { return base.Count; }
        }

        public MappingElement this[int index]
        {
            get { return (MappingElement) BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        new public MappingElement this[string name]
        {
            get { return (MappingElement) BaseGet(name); }
        }

        public int IndexOf(MappingElement url)
        {
            return BaseIndexOf(url);
        }

        public void Add(MappingElement url)
        {
            BaseAdd(url);
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        public void Remove(MappingElement url)
        {
            if (BaseIndexOf(url) >= 0)
                BaseRemove(url.BaseModuleName);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void Clear()
        {
            BaseClear();
        }

        [ConfigurationProperty("useBoundActivator", IsRequired = false, IsKey = false,DefaultValue = false)]
        public bool UseBoundActivator
        {
            get
            {
                if (base["useBoundActivator"].IsNull()) return false;
                return (bool)base["useBoundActivator"];
            }
            set { base["useBoundActivator"] = value; }
        }

        [ConfigurationProperty("bindPrivates", IsRequired = false, IsKey = false, DefaultValue = true)]
        public bool BindPrivates
        {
            get { return (bool)base["bindPrivates"]; }
            set { base["bindPrivates"] = value; }
        }
    }
}