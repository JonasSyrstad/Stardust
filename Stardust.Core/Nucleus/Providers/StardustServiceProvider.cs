using System;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Nucleus.Providers
{
    public class StardustServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            try
            {
                return Resolver.ResolverKernel.GetService(serviceType,TypeLocatorNames.DefaultName);
            }
            catch (ModuleCreatorException)
            {
                return null;
            }
        }
    }
}
