using System;
using Stardust.Interstellar.Messaging;
using Stardust.Nucleus;

namespace Stardust.Interstellar
{
    /// <summary>
    /// contains a set of extension methods to wrap up runtime executions 
    /// </summary>
    public static class ServiceTearDown
    {
        public static void TearDown(this IRuntime runtime, string payload)
        {
            Resolver.Activate<IServiceTearDown>().TearDown(runtime, payload);
        }
        
        public static T TearDown<T>(this IRuntime runtime, T message) where T : IResponseBase
        {
            return Resolver.Activate<IServiceTearDown>().TearDown(runtime, message);
        }
        
        public static Exception TearDown(this IRuntime runtime, Exception exception)
        {
            Resolver.Activate<IServiceTearDown>().TearDown(runtime, exception);
            return exception;
        }

    }
}