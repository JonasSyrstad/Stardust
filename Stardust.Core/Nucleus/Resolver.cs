//
// Resolver.cs
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
using System.Linq;
using System.Reflection;
using Stardust.Core.Wcf;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.DefaultImplementations;
using Stardust.Nucleus.ContainerIntegration;
using Stardust.Nucleus.ContextProviders;
using Stardust.Nucleus.Extensions;
using Stardust.Nucleus.Internals;
using Stardust.Nucleus.ObjectActivator;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Stardust.Nucleus
{
    /// <summary>
    /// Provides a fluent API to configure the ModuleCreator bindings programmaticaly.
    /// </summary>
    /// <remarks>Note that xml configuration will override these settings</remarks>
    public static class Resolver
    {

        public static IConfigurator GetConfigurator()
        {
            if (Configurator != null && OptimizedKernel)
                return Configurator;
            if (OptimizedKernel)
            {
                Configurator = KernelFactory.CreateConfigurator(ConfigurationKernel);
                return Configurator;
            }
            return KernelFactory.CreateConfigurator(ConfigurationKernel);
        }



        private static Scope KernelScope = Scope.Singleton;
        private static readonly object Triowing = new object();
        private static IConfigurationKernel SingletonConfigurationKernel;
        private static bool OptimizedKernel = ConfigurationManagerHelper.GetValueOnKey("stardust.UseTransientKernel") != "true";
        private static IDependencyResolver ResolverKernel1;
        private static IConfigurator Configurator;
        private static bool HasExternalIoc;

        public static void SetTransientKernel()
        {
            OptimizedKernel = false;
        }
        /// <summary>
        /// Creates a new kernel within an extended scope
        /// </summary>
        /// <param name="kernelScope">The scope to create the kernel context in</param>
        /// <returns></returns>
        public static IKernelContext BeginKernelScope<T>(Scope kernelScope) where T : ILogging
        {
            if (kernelScope == Scope.PerRequest) throw new StardustCoreException("Cannot begin a kernel context in PerRequest scope");
            KernelScope = kernelScope;
            var kernelContext = new KernelContext(ContainerFactory.Current.GetProvider(KernelScope).BeginExtendedScope());
            LoadKernel();
            return kernelContext;
        }

        /// <summary>
        /// Creates a new kernel context within an extended scope
        /// </summary>
        /// <returns></returns>
        public static IKernelContext BeginKernelScope<T>() where T : ILogging
        {
            return BeginKernelScope<T>(Scope.Singleton);
        }

        public static IKernelContext BeginKernelScope(Scope kernelScope)
        {
            return BeginKernelScope<LoggingDefaultImplementation>(kernelScope);
        }

        public static IKernelContext BeginKernelScope()
        {
            return BeginKernelScope<LoggingDefaultImplementation>();
        }

        /// <summary>
        /// Terminates the kernel context
        /// </summary>
        /// <param name="provider"></param>
        public static void EndKernelScope(IKernelContext provider, bool cleanAllScopes)
        {
            ((IKernelContextCommands)provider).End();
            if (cleanAllScopes)
                ContainerFactory.Current.KillAllInstances();
        }

        internal static IConfigurationKernel ConfigurationKernel
        {
            get
            {
                if (KernelScope == Scope.Singleton && SingletonConfigurationKernel != null && OptimizedKernel)
                    return SingletonConfigurationKernel;
                var k = LoadKernel();
                return k;
            }
        }

        internal static IDependencyResolver ResolverKernel
        {
            get
            {
                if (ResolverKernel1 != null && OptimizedKernel)
                {
                    return ResolverKernel1;

                }
                if (OptimizedKernel)
                {
                    ResolverKernel1 = ResolverKernel1 = KernelFactory.CreateResolver(ConfigurationKernel);
                    return ResolverKernel1;
                }
                return KernelFactory.CreateResolver(ConfigurationKernel);
            }
        }


        private static IConfigurationKernel LoadKernel()
        {
            if (SingletonConfigurationKernel.IsInstance() && KernelScope == Scope.Singleton && OptimizedKernel) return SingletonConfigurationKernel;
            var kernel = (IConfigurationKernel)ContainerFactory.Current.Resolve(typeof(IConfigurationKernel), KernelScope);
            if (kernel.IsInstance()) return kernel;
            lock (Triowing)
            {
                var newKernel = KernelFactory.CreateKernel();
                kernel = (IConfigurationKernel)ContainerFactory.Current.Resolve(typeof(IConfigurationKernel), KernelScope);
                if (kernel.IsInstance()) return kernel;
                ContainerFactory.Current.Bind(typeof(IConfigurationKernel), newKernel, KernelScope);
                if (SingletonConfigurationKernel == null && KernelScope == Scope.Singleton && OptimizedKernel)
                    SingletonConfigurationKernel = newKernel;
            }
            return kernel;
        }

        /// <summary>
        /// Creates an instance of the resolved type as its base type or interface
        /// </summary>
        public static T Activate<T>(string named)
        {
            return ResolverKernel.GetService<T>(named);
            //return Resolve<T>(named).Activate();
        }

        /// <summary>
        /// Creates an instance of the resolved type as its base type or interface
        /// </summary>
        public static T Activate<T>(Enum selector)
        {
            return Activate<T>(selector.ToString());
        }

        /// <summary>
        /// Creates an instance of the resolved type as its base type or interface
        /// </summary>
        public static T Activate<T>()
        {
            return ResolverKernel.GetService<T>();
        }

        /// <summary>
        /// Creates an instance of the resolved type as its base type or interface
        /// </summary>
        public static T Activate<T>(string named, Action<T> initializer)
        {
            return ResolverKernel.GetService<T>(named, initializer);
        }

        /// <summary>
        /// Creates an instance of the resolved type as its base type or interface
        /// </summary>
        public static T Activate<T>(Enum selector, Action<T> initializer)
        {
            return Activate(selector.ToString(), initializer);
        }

        /// <summary>
        /// Creates an instance of the resolved type as its base type or interface
        /// </summary>
        public static T Activate<T>(Action<T> initializer)
        {
            return ResolverKernel.GetService(initializer);
        }

        /// <summary>
        /// Resolves a implementation binding by the root class or interface
        /// </summary>
        public static T ActivateGeneric<T>(string named)
        {
            var type = new ResolveContext<T>((IScopeContextInternal)ConfigurationKernel.Resolve(typeof(T).GetGenericTypeDefinition(), named));
            return type.Activate();
        }

        public static T ActivateGeneric<T>(Enum typed)
        {
            return ActivateGeneric<T>(typed.ToString());
        }

        /// <summary>
        /// Resolves a implementation binding by the root class or interface
        /// </summary>
        public static T ActivateGeneric<T>()
        {
            return ActivateGeneric<T>(TypeLocatorNames.DefaultName);
        }

        /// <summary>
        /// Lists all registered implementations of an interface
        /// </summary>
        public static IEnumerable<KeyValuePair<string, string>> GetImplementationsOf<T>()
        {
            return ConfigurationKernel.ResolveList(typeof(T));
        }

        /// <summary>
        /// This method contains all binding configuration on one easy to use file. Add this statement to your applications startup code block (like Global.asax+Application_Start)
        /// </summary>
        /// <param name="config">an implementation of IModuleConfiguration that contains all bindings</param>
        public static void LoadModuleConfiguration(IBlueprint config)
        {
            Logging.SetLogger(config.LoggingType);
            HasExternalIoc = config is IContainerSetup;
            KernelFactory.LoadContainer(config);
            LoadKernel();
            GetConfigurator().Bind<ILogging>().To(config.LoggingType).SetSingletonScope();
            config.Bind(GetConfigurator());
        }

        public static void LoadIocContainer(IContainerSetup containerSetup)
        {
            HasExternalIoc = true;
            KernelFactory.LoadContainer(containerSetup);
        }

        /// <summary>
        /// This method contains all binding configuration on one easy to use file. Add this statement to your applications startup code block (like Global.asax+Application_Start)
        /// </summary>
        /// <typeparam name="T">Your module configuration class</typeparam>
        public static void LoadModuleConfiguration<T>() where T : IBlueprint, new()
        {
            LoadModuleConfiguration(new T());
        }

        /// <summary>
        /// Loads the binding and IOC bridge based on configuration settings. There is no need to call this manually as it is run in pre app start.
        /// You can still add more binding files in you app start method. Even change the settings loaded in this. But be carefull not to brake the SOLID principles by modifying behavior at runtime.
        /// </summary>
        public static void LoadModuleConfiguration()
        {
            var c = Configuration.ConfigurationHelper.Configurations.Value;
            if (c.IocBridgeFactory != null)
                KernelFactory.LoadContainer((IContainerSetup)ObjectFactory.Createinstance(c.IocBridgeFactory, new object[0]));
            if (c.ConfigurationReaderType != null)
            {
                GetConfigurator().Bind<IConfigurationReader>().To(c.ConfigurationReaderType).SetSingletonScope();
                GetConfigurator().Bind<IRuntimeContext>().To<DefaultRuntimeContext>().SetTransientScope();
            }

            if (c.BindingConfigurationType != null)
                LoadModuleConfiguration((IBlueprint)ObjectFactory.Createinstance(c.BindingConfigurationType, new object[0]));
        }

        public static void RemoveAll()
        {
            GetConfigurator().RemoveAll();
        }

        /// <summary>
        /// Revert to the default activator
        /// </summary>
        public static void ResetActivator()
        {
            ActivatorFactory.ResetActivator();
        }

        /// <summary>
        /// Sets the default activationscope
        /// </summary>
        /// <param name="activationScope"></param>
        public static void SetDefaultActivationScope(Scope activationScope)
        {
            ScopeContext.SetDefaultActivationScope(activationScope);
        }


        internal static IEnumerable<T> ActivateAll<T>()
        {
            return ResolverKernel.GetServices<T>();
        }

        public static object Activate(Type serviceType, string key)
        {
            return ResolverKernel.GetService(serviceType, key);
        }

        public static object Activate(Type serviceType)
        {
            return ResolverKernel.GetService(serviceType, TypeLocatorNames.DefaultName);
        }

        public static IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return ResolverKernel.GetServices(serviceType);
        }

        public static IEnumerable<TService> GetAllInstances<TService>()
        {
            return ResolverKernel.GetServices<TService>();
        }

        public static Dictionary<string,object> GetAllInstancesNamed(Type serviceType)
        {
            return ResolverKernel.GetServicesNamed(serviceType);
        }

        public static Dictionary<string, TService> GetAllInstancesNamed<TService>()
        {
            return ResolverKernel.GetServicesNamed<TService>(null);
        }

        public static Dictionary<string, TService> GetAllInstancesNamed<TService>(string exceptWithName)
        {
            return ResolverKernel.GetServicesNamed<TService>(exceptWithName);
        }

        /// <summary>
        /// Creates a new nub OLM scope
        /// </summary>
        /// <param name="scope">the scope to extend</param>
        /// <returns></returns>
        public static IExtendedScopeProvider BeginExtendedScope(Scope scope)
        {
            return ResolverKernel.BeginExtendedScope(ContainerFactory.Current.ExtendScope(scope));
        }
    }

    public static class BinderExtensions
    {
        public static void ToAssembly<T>(this IBindContext<T> self, string assemblyName, Action<IScopeContext> scopeHandler = null)
        {
            var assembly = Assembly.Load(assemblyName);
            self.ToAssembly(assembly, scopeHandler);
        }

        public static void ToAssembly<T>(this IBindContext<T> self, Assembly assembly, Action<IScopeContext> scopeHandler = null)
        {
            foreach (var definedType in assembly.DefinedTypes)
            {
                if (!definedType.Implements(typeof(T))) continue;
                var attrib = definedType.GetAttribute<ImplementationKeyAttribute>();
                IScopeContext scope;
                if (attrib != null) scope = self.To(definedType, attrib.Name);
                else scope = self.To(definedType);
                if (scopeHandler != null) scopeHandler(scope);
            }
        }

        public static IConfigurator LoadAssembly(this IConfigurator self, string name)
        {
            Assembly.Load(new AssemblyName(name));
            return self;
        }

        public static IConfigurator LoadAssemblyFromPath(this IConfigurator self, string path)
        {
            Assembly.LoadFrom(path);
            return self;
        }

        /// <summary>
        /// Resolves all types in the loaded assemblies
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IConfigurator ResolveAll(this IConfigurator self, bool onlyInterfacesOrAbstracts = true, Action<IScopeContext> scopeHandler = null)
        {
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                self.ResolveAssembly(onlyInterfacesOrAbstracts, assembly, scopeHandler);
            }
            return self;
        }

        public static IConfigurator ResolveAssembly(this IConfigurator self, bool onlyInterfacesOrAbstracts, string assemblyName, Action<IScopeContext> scopeHandler = null)
        {
            var assembly = Assembly.Load(new AssemblyName(assemblyName));
            return self.ResolveAssembly(onlyInterfacesOrAbstracts, assembly);
        }

        public static IConfigurator ResolveAssembly(this IConfigurator self, bool onlyInterfacesOrAbstracts, Assembly assembly, Action<IScopeContext> scopeHandler = null)
        {
            if (scopeHandler == null) scopeHandler = a => { a.SetTransientScope(); };
            foreach (var definedType in assembly.DefinedTypes)
            {
                if (onlyInterfacesOrAbstracts && !definedType.IsConcreteType())
                {
                    continue;
                }
                var name = definedType.GetAttribute<ImplementationKeyAttribute>() != null ? definedType.GetAttribute<ImplementationKeyAttribute>().Name : TypeLocatorNames.DefaultName;
                var implementations = FindImplementations(definedType);
                foreach (var implementation in implementations)
                {
                    scopeHandler(definedType.IsGenericTypeDefinition ? self.BindAsGeneric(definedType).To(implementation, name) : self.Bind(definedType).To(definedType, name));
                }
            }
            return self;
        }

        private static IEnumerable<TypeInfo> FindImplementations(TypeInfo definedType)
        {
            return from a in AppDomain.CurrentDomain.GetAssemblies() 
                   from t in a.DefinedTypes 
                   where t.Implements(definedType) && t.IsConcreteType() 
                   select t;
        }
    }

}