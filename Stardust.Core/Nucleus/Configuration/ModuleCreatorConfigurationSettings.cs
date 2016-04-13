//
// ModuleCreatorConfigurationSettings.cs
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
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Stardust.Particles;

namespace Stardust.Nucleus.Configuration
{
    public class StringToTypeTypeConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return value.IsNull() ? null : Type.GetType(value.ToString(), true, true);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }
    }
    [ExcludeFromCodeCoverage]
    public sealed class ModuleCreatorConfigurationSettings : ConfigurationSection
    {
        static ModuleCreatorConfigurationSettings()
        {
            TypeDescriptor.AddAttributes(typeof (Type), new TypeConverterAttribute(typeof (StringToTypeTypeConverter)));
        }
        [ConfigurationProperty("moduleCreators", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(ModuleCollection), AddItemName = "module")]
        public ModuleCollection ModuleCreators
        {

            get
            {
                var mappingCollection = (ModuleCollection)base["moduleCreators"];
                return mappingCollection;
            }
        }

        [ConfigurationProperty("bindingConfigurationType")]
        public string BindingConfigurationTypeName
        {
            get { return (string)this["bindingConfigurationType"]; }
            set { this["bindingConfigurationType"] = value; }
        }

        [ConfigurationProperty("readerType")]
        public string ConfigurationReaderTypeName
        {
            get { return (string)this["readerType"]; }
            set { this["readerType"] = value; }
        }

        [ConfigurationProperty("iocBridgeFactory")]
        public string IocBridgeFactoryName
        {
            get { return (string)this["iocBridgeFactory"]; }
            set { this["iocBridgeFactory"] = value; }
        }

        internal Type ConfigurationReaderType
        {
            get
            {
                if (ConfigurationReaderTypeName.IsNullOrWhiteSpace()) return null;
                try
                {
                    return Type.GetType(ConfigurationReaderTypeName);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        internal Type BindingConfigurationType
        {
            get
            {
                if (BindingConfigurationTypeName.IsNullOrWhiteSpace()) return null;
                try
                {
                    return Type.GetType(BindingConfigurationTypeName);
                }
                catch (Exception)
                {
                    return null;
                }

            }
        }

        internal Type IocBridgeFactory
        {
            get
            {
                if (BindingConfigurationTypeName.IsNullOrWhiteSpace()) return null;
                try
                {
                    return Type.GetType(IocBridgeFactoryName);
                }
                catch (Exception)
                {
                    return null;
                }

            }
        }

        protected override string SerializeSection(ConfigurationElement parentElement, string name, ConfigurationSaveMode saveMode)
        {
            var s = base.SerializeSection(parentElement, name, saveMode);
            return s;
        }

        public static ModuleCreatorConfigurationSettings GetSettings()
        {
            try
            {
                var kreatorSettings = ConfigurationManager.GetSection("moduleCreator") as ModuleCreatorConfigurationSettings;
                return kreatorSettings;
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
