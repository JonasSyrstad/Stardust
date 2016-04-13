using Stardust.Interstellar;

namespace Stardust.Core.Service.Web
{
    internal static class RuntimeHelpers
    {
        internal static IServiceContainer<T> GetDelegateService<T>(this IRuntime runtime) where T : class
        {
            var container = runtime.CreateServiceProxy<T>();
            container.Initialize(runtime.GetBootstrapContext());
            return container;
        }

        internal static IServiceContainer<T> GetSecuredService<T>(this IRuntime runtime) where T : class
        {
            var container = runtime.CreateServiceProxy<T>();
            container.Initialize(true);
            return container;
        }
    }
}