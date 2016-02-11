//
// DefaultComponentRegistration.cs
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
using System.IO;
using System.Linq;
using System.Reflection;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;
using Stardust.Particles.Xml;

namespace Stardust.Clusters
{
    public class DefaultComponentRegistration : IComponentRegistration
    {
        private const string NoTypeFoundMessage = "No classes that implement {0} was found";
        private Assembly ImportedAssembly;
        private ConfiguredItems ModuleList;
        private string RootPath;

        private string ConfigFile
        {
            get { return Path.Combine(RootPath, "moduleStore.xml"); }
        }

        public IComponentRegistration Enumerate()
        {
            if (File.Exists(ConfigFile))
                ModuleList = Deserializer<ConfiguredItems>.GetInstanceFromFile(ConfigFile);
            else
                ModuleList = new ConfiguredItems { Items = new List<ConfiguredItem>() };
            return this;
        }

        public ObjectInitializer[] GetAllRegisteredImplementationsFor<T>()
        {
            var items = from i in ModuleList.Items
                        where i.BaseType == typeof(T).FullName
                        select i.ObjectInitializer;
            return items.ToArray();
        }

        public ObjectInitializer GetObjectInitializerFor<T>(string moduleName)
        {
            var item = GetItem<T>(moduleName);
            return item.First().ObjectInitializer;
        }

        public IComponentRegistration ImportAssemblyWith<T>(string registrationName, byte[] assembly)
        {
            return ImportAssemblyWith<T>(registrationName, assembly, null);
        }

        public IComponentRegistration ImportAssemblyWith<T>(string registrationName, byte[] assembly, string className)
        {
            return ImportAssemblyWith(typeof(T), registrationName, assembly, className);
        }

        public IComponentRegistration ImportAssemblyWith(Type implementationToRegister, string registrationName, byte[] assembly)
        {
            return ImportAssemblyWith(implementationToRegister, registrationName, assembly, null);
        }

        public IComponentRegistration ImportAssemblyWith(Type implementationToRegister, string registrationName, byte[] assembly, string className)
        {
            var module = Assembly.Load(assembly);
            ImportedAssembly = module;
            SaveAssemblyToPath(assembly, module);
            return ScanFor(implementationToRegister, registrationName, className, module);
        }

        public bool IsInitialized()
        {
            return RootPath.ContainsCharacters()
                && ModuleList.IsInstance();
        }

        public IComponentRegistration ScanImportedAssemblyFor<T>(string registrationName)
        {
            return ScanImportedAssemblyFor<T>(registrationName, null);
        }

        public IComponentRegistration ScanImportedAssemblyFor<T>(string registrationName, string className)
        {
            return ScanImportedAssemblyFor(typeof(T), registrationName, className);
        }

        public IComponentRegistration ScanImportedAssemblyFor(Type implementationToRegister, string registrationName)
        {
            return ScanImportedAssemblyFor(implementationToRegister, registrationName, null);
        }

        public IComponentRegistration ScanImportedAssemblyFor(Type implementationToRegister, string registrationName, string className)
        {
            if (ImportedAssembly.IsNull()) throw new StardustCoreException("No assembly imported");
            return ScanFor(implementationToRegister, registrationName, className, ImportedAssembly);
        }

        public IComponentRegistration SetFolder(string rootPath)
        {
            RootPath = rootPath;
            return this;
        }

        public IComponentRegistration Update()
        {
            ModuleList.SerializeToFile(ConfigFile, false);
            return this;
        }

        private static string ConstructAssemblyFolderName(Assembly module)
        {
            return module.FullName.Replace(",", "_").Replace(" ", "");
        }

        private static Type FindType(Type typeToFind, Assembly module, string className)
        {
            var types = TypeExtractor.RetreiveSubTypesFromAssembly(typeToFind, module);
            if (className.ContainsCharacters())
                types = from t in types
                        where t.FullName == className
                        select t;
            var type = types.FirstOrDefault();
            if (type.IsNull())
                throw new StardustCoreException(NoTypeFoundMessage.FormatString(typeToFind.FullName));
            return type;
        }

        private ObjectInitializer CreateObjectInitializer(string registrationName, Type type, Assembly module)
        {
            return new ObjectInitializer
                       {
                           FullName = type.FullName,
                           AssemblyPath = GetAssemblyPath(module),
                           AssemblyName = module.FullName,
                           Name = registrationName,
                           ModuleType = type
                       };
        }

        private string GetAssemblyPath(Assembly module)
        {
            return Path.Combine(RootPath, ConstructAssemblyFolderName(module), module.GetName().Name + ".dll");
        }

        private IEnumerable<ConfiguredItem> GetItem(Type implementationToRegister, string moduleName)
        {
            var item = from i in ModuleList.Items
                       where i.ObjectInitializer.Name == moduleName && i.BaseType == implementationToRegister.FullName
                       select i;
            return item;
        }

        private IEnumerable<ConfiguredItem> GetItem<T>(string moduleName)
        {
            var item = from i in ModuleList.Items
                       where i.ObjectInitializer.Name == moduleName && i.BaseType == typeof(T).FullName
                       select i;
            return item;
        }

        private IComponentRegistration Register(Type implementationToRegister, string registrationName, Assembly module, ConfiguredItem item, Type type)
        {
            if (item.IsInstance())
                RemoveItem(item);
            ModuleList.Items.Add(new ConfiguredItem
            {
                BaseType = implementationToRegister.FullName,
                ObjectInitializer = CreateObjectInitializer(registrationName, type, module)
            });
            return this;
        }

        private void RemoveItem(ConfiguredItem item)
        {
            ModuleList.Items.RemoveAt(ModuleList.Items.IndexOf(item));
        }

        private void SaveAssemblyToPath(byte[] assembly, Assembly module)
        {
            var path = Path.Combine(RootPath, ConstructAssemblyFolderName(module));
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (!File.Exists(GetAssemblyPath(module)))
                File.WriteAllBytes(GetAssemblyPath(module), assembly);
        }

        private IComponentRegistration ScanFor(Type implementationToRegister, string registrationName, string className, Assembly module)
        {
            var type = FindType(implementationToRegister, module, className);
            var item = GetItem(implementationToRegister, registrationName).FirstOrDefault();
            return Register(implementationToRegister, registrationName, module, item, type);
        }
    }
}