using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Stardust.Nucleus;

namespace Stardust.Core.ServiceLocatorAdapter
{
    internal class StardustServiceLocatorAdapter:IServiceLocator
    {
        public object GetService(Type serviceType)
        {
            return Resolver.Activate(serviceType);
        }

        public object GetInstance(Type serviceType)
        {
            return GetService(serviceType);
        }

        public object GetInstance(Type serviceType, string key)
        {
            return Resolver.Activate(serviceType,key);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return Resolver.GetAllInstances(serviceType);
        }

        public TService GetInstance<TService>()
        {
            return Resolver.Activate<TService>();
        }

        public TService GetInstance<TService>(string key)
        {
            return Resolver.Activate<TService>(key);
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return Resolver.GetAllInstances<TService>();
        }
    }
}
