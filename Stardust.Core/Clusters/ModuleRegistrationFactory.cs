//
// moduleregistrationfactory.cs
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

using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Clusters
{
    /// <summary>
    /// Implement IComponentRegistration to write your custom Component Registration module, 
    /// this may store metadata in a database while the files are stored on disk.
    /// Do a call to SetFolder and  Enumerate in application start.
    /// </summary>
    public static class ModuleRegistrationFactory
    {
        private const string NotInitializedMessage = "Component registration module is not initialized";
        private static IComponentRegistration ComponentHandler;


        /// <summary>
        /// Creates a new instance of <see cref="IComponentRegistration"/>. 
        /// Use this if you need more than one instance of the interface that stores files in separate locations. 
        /// The instance is initialized if you pass a value in path.
        /// </summary>
        public static IComponentRegistration CreateNewComponentRegistration(string path)
        {
            var item = Resolver.Activate<IComponentRegistration>();
            if (path.IsNullOrWhiteSpace()) return item;   
            return item.SetFolder(path).Enumerate();
        }

        /// <summary>
        /// Initialized the component handler with the location of the component store.
        /// </summary>
        /// <param name="path"></param>
        public static void Initialize<T>(string path) where T : IComponentRegistration
        {
            ResetHandlerTo<T>();
            ComponentHandler.SetFolder(path).Enumerate();
        }

        /// <summary>
        /// Initialized the component handler with the location of the component store.
        /// </summary>
        /// <param name="path"></param>
        public static void Initialize(string path)
        {
            Initialize<DefaultComponentRegistration>(path);
        }

        public static void ReEnumerate()
        {
            Validate();
            ComponentHandler.Enumerate();
        }

        private static void Validate()
        {
            if (!ComponentHandler.IsInitialized())
                throw new StardustCoreException(NotInitializedMessage);
        }

        public static void ImportAssemblyWith<T>(string registrationName, byte[] assembly)
        {
            ImportAssemblyWith<T>(registrationName,assembly,null);
        }

        public static void ImportAssemblyWith<T>(string registrationName, byte[] assembly,string className)
        {
            Validate();
            ComponentHandler.ImportAssemblyWith<T>(registrationName, assembly,className);
            ComponentHandler.Update();
        }

        public static ObjectInitializer GetObjectInitializerFor<T>(string moduleName)
        {
            return ComponentHandler.GetObjectInitializerFor<T>(moduleName);
        }

        public static T GetInstance<T>(ObjectInitializer objectInitializer)
        {
            return (T) objectInitializer.ModuleType.Activate(objectInitializer.Scope);
        }

        public static T GetInstance<T>(string moduleName)
        {
            var objectInitializer = GetObjectInitializerFor<T>(moduleName);
            return GetInstance<T>(objectInitializer);
        }

        private static void ResetHandler()
        {
            ComponentHandler = Resolver.Activate<IComponentRegistration>();
        }

        public static void ResetHandlerTo<T>() where T : IComponentRegistration
        {
            Resolver.GetConfigurator().UnBind<IComponentRegistration>()
                .AllAndBind()
                .To<T>().SetTransientScope().DisableOverride();
            ResetHandler();
        }

        public static string GetComponentRegistrationName()
        {
            return ComponentHandler.GetType().FullName;
        }

        public static bool UsesDefaultComponentRegistrationModule()
        {
            return ComponentHandler.GetType() == typeof(DefaultComponentRegistration);
        }

        public static void SetDefaultComponentRegistrationModule()
        {
            ResetHandlerTo<DefaultComponentRegistration>();
        }

        public static ObjectInitializer[] GetAllRegisteredImplementationsFor<T>()
        {
            return ComponentHandler.GetAllRegisteredImplementationsFor<T>();
        }
    }
}