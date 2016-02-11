//
// DynamicConfigurableObjectBase.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Stardust.Particles
{
    public abstract class DynamicConfigurableObjectBase : IDynamicConfigurableObject
    {
        private const string InvalidDataTypeErrorMessage = "Property '{0}' should be of type {1}, not {2}";
        private const string InvalidPropertyNameErrorMessage = "Invalid property name '{0}'";
        private readonly Dictionary<string, object> PropertyValues = new Dictionary<string, object>();
        private readonly Dictionary<string, Type> PropertyNames;

        /// <summary>
        /// Pass 'null' if your implementation does not have dynamic properties.
        /// Create a static method that returns a Dictionary with the names and types of dynamic properties you need.
        /// </summary>
        protected DynamicConfigurableObjectBase(Dictionary<string, Type> propertyNames)
        {
            if (propertyNames.IsNull())
                PropertyNames = new Dictionary<string, Type>();
            PropertyNames = propertyNames;
        }

        protected object GetPropertyValue(string name)
        {
            if (PropertyValues.ContainsKey(name))
                return PropertyValues[name];
            return null;
        }

        protected virtual T GetPropertyValue<T>(string name)
        {
            if (PropertyValues.ContainsKey(name))
                return (T)PropertyValues[name];
            return default(T);
        }

        protected bool ContainsValueFor(string name)
        {
            return PropertyValues.ContainsKey(name);
        }

        public void SetProperty<T>(string name, T value)
        {
            ValidatePropertyValue(name, value);
            PropertyValues.AddOrUpdate(name, value);
        }

        private void ValidatePropertyValue(string name, object value)
        {
            if (!PropertyNames.ContainsKey(name))
                throw new StardustCoreException(String.Format(InvalidPropertyNameErrorMessage, name));
            if (PropertyNames[name] != value.GetType())
                throw new StardustCoreException(String.Format(InvalidDataTypeErrorMessage, name, PropertyNames[name].Name, value.GetType().Name));
        }

        [ExcludeFromCodeCoverage]
        public void SetProperties(params KeyValuePair<string, object>[] values)
        {
            foreach (var keyValuePair in values)
                SetProperty(keyValuePair.Key, keyValuePair.Value);
        }

        public KeyValuePair<string, Type>[] GetConfigurableProperties()
        {
            return (from pn in PropertyNames select pn).ToArray();
        }

        public bool HasExtendedProperties()
        {
            return PropertyNames.Any();
        }
    }

    [ExcludeFromCodeCoverage]
    public class DynamicConfigurableObjectBase<T> : DynamicConfigurableObjectBase where T :class, IValidateableDto
    {
        private const string Property = "Property";

        public DynamicConfigurableObjectBase()
            : base(GetParameter())
        { }

        private static Dictionary<string, Type> GetParameter()
        {
            return new Dictionary<string, Type> { { Property, typeof(T) } };
        }

        protected T GetProperty()
        {
            return GetPropertyValue<T>(Property);
        }

        protected override TT GetPropertyValue<TT>(string name)
        {
            var result = base.GetPropertyValue<TT>(name);
            var t =result as T;
            if (t.IsNull()) throw new StardustCoreException("Invalid type");
            t.Validate();
            return result;
        }

        public void SetProperty(T property)
        {
            property.Validate();
            SetProperty(Property, property);
        }
    }
}