using System;
using System.IdentityModel.Tokens;
using System.Web;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Particles;
using Utilities = Stardust.Interstellar.Utilities.Utilities;

namespace Stardust.Core.Service.Web
{
    internal class ControllerActivatorBase
    {
        private static ISupportCodeGenerator generator;

        private static bool initialized;

        protected ISupportCodeGenerator Generator
        {
            get
            {
                try
                {
                    if (generator == null && !initialized)
                    {
                        generator = Resolver.Activate<ISupportCodeGenerator>();
                        initialized = true;
                    }
                    return generator;
                }
                catch (Exception ex)
                {
                    ex.Log();
                    return null;
                }
            }
        }

        protected internal void Initialize(Uri uri, string action, IStardustController controllerInitializer)
        {
            controllerInitializer.Runtime.SetEnvironment(Utilities.GetEnvironment());
            var serviceName = controllerInitializer.GetServiceName(uri);
            var tracer = controllerInitializer.Runtime.SetServiceName(controllerInitializer, Utilities.GetServiceName(),
                controllerInitializer.GetMethodName(uri, action));
            tracer.GetCallstack().Name = serviceName;
            controllerInitializer.SetTracer(tracer);
            controllerInitializer.Runtime.SetCurrentPrincipal(HttpContext.Current.User);
            var supportCode = CreateSupportCode();
            controllerInitializer.Runtime.TrySetSupportCode(supportCode);
            if (controllerInitializer.Runtime.GetCurrentClaimsIdentity().IsInstance() &&
                controllerInitializer.Runtime.GetCurrentClaimsIdentity().BootstrapContext.IsInstance())
                controllerInitializer.Runtime.SetBootstrapContext(TryGetBootstrapContext(controllerInitializer.Runtime));
            if (HttpContext.Current != null && controllerInitializer.Runtime.GetStateStorageContainer() != null)
                controllerInitializer.Runtime.GetStateStorageContainer().TryAddStorageItem(HttpContext.Current, "httpContext");
        }

        private string CreateSupportCode()
        {
            try
            {
                var supportCode = GetSupportCodeFromHeader();
                if (supportCode.ContainsCharacters())
                {
                    return supportCode;
                }
                if (Generator != null)
                {
                    return Generator.CreateSupportCode();
                }
                return Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                ex.Log();
                return Guid.NewGuid().ToString();
            }
        }

        private static string GetSupportCodeFromHeader()
        {
            try
            {
                var val = HttpContext.Current.Request.Headers[BaseApiController.SupportCodeHeaderName];
                return val;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static BootstrapContext TryGetBootstrapContext(IRuntime runtime)
        {
            if (runtime.GetCurrentClaimsIdentity().IsInstance() &&
                runtime.GetCurrentClaimsIdentity().BootstrapContext.IsInstance())
                return runtime.GetCurrentClaimsIdentity().BootstrapContext as BootstrapContext;
            return null;
        }
    }
}