using Microsoft.Practices.ServiceLocation;
using Stardust.Particles;

namespace Stardust.Core.ServiceLocatorAdapter
{
    /// <summary>
    /// Register Stardust as the service locator provider.
    /// </summary>
    public static class RegisterServiceLocaterAdapter
    {
        private static IServiceLocator Locator1;

        /// <summary>
        /// Register Stardust as the service locator provider.
        /// </summary>
        public static void RegisterProvider()
        {
            ServiceLocator.SetLocatorProvider(GetProviderFactory());
        }

        /// <summary>
        /// Creates a <see cref="IServiceLocator"/> factory instance
        /// </summary>
        /// <returns>A factory delegate</returns>
        public static ServiceLocatorProvider GetProviderFactory()
        {
            return () => Locator;
        }

        private static IServiceLocator Locator
        {
            get
            {
                if(Locator1.IsNull())
                    Locator1=new StardustServiceLocatorAdapter();
                return Locator1;
            }
        }
    }
}