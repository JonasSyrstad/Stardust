using System;
using Stardust.Nucleus.ContainerIntegration;

namespace Stardust.Nucleus.TypeResolver
{
    internal static class KernelFactory
    {
        public static object Triowing=new object();
        private static Type KernelType = typeof (TypeResolverConfigurationKernel);
        private static Type OptimizerType = typeof (TypeLocatorOptimizer);
        private static IContainerSetup IocFactory;

        /// <summary>
        /// Creates a new resolver kernel
        /// </summary>
        /// <returns></returns>
        internal static IConfigurationKernel CreateKernel()
        {
            if(IocFactory==null) LoadContainer();            
            return IocFactory.GetKernel();
        }

        private static void LoadContainer()
        {
            lock (Triowing)
            {
                if(IocFactory!=null) return;
                IocFactory = new StardustIocFactory();
            }
        }

        internal static IDependencyResolver CreateResolver(IConfigurationKernel configurationKernel)
        {
            if (IocFactory == null) LoadContainer();   
                return IocFactory.GetResolver(configurationKernel);
        }

        internal static void LoadContainer(IBlueprint config)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global 
            lock (Triowing)
            {
                if (IocFactory != null) return;
                IocFactory = config as IContainerSetup ?? new StardustIocFactory(); 
            }
        }

        internal static void LoadContainer(IContainerSetup containerSetup)
        {
            lock (Triowing)
            {
                if(IocFactory!=null) throw new InvalidOperationException("Container factory already created");
                IocFactory = containerSetup;
            }
        }

        internal static IConfigurator CreateConfigurator(IConfigurationKernel configurationKernel)
        {
            if (IocFactory == null) LoadContainer();   
            return IocFactory.GetConfigurator(configurationKernel);
        }
    }
}