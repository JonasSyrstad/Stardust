using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Stardust.Core.CrossCutting;
using Stardust.Core.FactoryHelpers;
using Stardust.Core.Wcf;

namespace Stardust.Core.Web.Mvc
{
    public class MvcControllerResolver : IDependencyResolver
    {
        private const string FailedToCreateInstance = "Failed to create instance of {0}";
        private readonly IDependencyResolver DefaultDependencyResolver;

        public MvcControllerResolver(IDependencyResolver dependencyResolver)
        {
            DefaultDependencyResolver = dependencyResolver;
        }

        public object GetService(Type serviceType)
        {
            if (!UseDefalutImplementation(serviceType))
                return GetInstance(serviceType);
            return DefaultDependencyResolver.GetService(serviceType);
        }

        private static object GetInstance(Type serviceType)
        {
            var serviceInterface = serviceType.GetInterface();
            object instance;
            if (serviceInterface.IsInstance())
                instance = Resolver.Activate(serviceInterface);
            else
                instance = Resolver.Activate(serviceType);
            if (instance.IsNull())
                throw new ModuleCreatorException(String.Format(FailedToCreateInstance, serviceType.Name));
            return instance;
        }

        private static bool UseDefalutImplementation(Type serviceType)
        {
            return serviceType == typeof(IViewPageActivator)
                   || serviceType == typeof(IControllerFactory)
                   || serviceType == typeof(IFilterProvider)
                   || serviceType.Name.Contains("cshtml")
                   || serviceType == typeof(ModelMetadataProvider);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var instances = DefaultDependencyResolver.GetServices(serviceType);
            return instances;
        }
    }
}
