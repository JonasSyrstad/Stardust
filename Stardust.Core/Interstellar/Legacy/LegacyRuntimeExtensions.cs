using System;
using Stardust.Nucleus;
using Stardust.Nucleus.Internals;

namespace Stardust.Interstellar.Legacy
{
    public static class LegacyRuntimeExtensions
    {

        public static TTask CreateRuntimeTask<TTask>(this IRuntime runtime) where TTask : IRuntimeTask
        {
            return runtime.CreateRuntimeTask<TTask>(ObjectInitializer.Default.Name, ScopeContext.GetDefaultScope());
        }

        public static TTask CreateRuntimeTask<TTask>(this IRuntime runtime, Scope scope) where TTask : IRuntimeTask
        {
            return runtime.CreateRuntimeTask<TTask>(ObjectInitializer.Default.Name, scope);
        }

        public static TTask CreateRuntimeTask<TTask>(this IRuntime runtime, Enum implementationRef) where TTask : IRuntimeTask
        {
            return runtime.CreateRuntimeTask<TTask>(implementationRef.ToString(), ScopeContext.GetDefaultScope());
        }

        public static TTask CreateRuntimeTask<TTask>(this IRuntime runtime, Enum implementationRef, Scope scope) where TTask : IRuntimeTask
        {
            return runtime.CreateRuntimeTask<TTask>(implementationRef.ToString(), scope);
        }

        public static TTask CreateRuntimeTask<TTask>(this IRuntime runtime, string implementationRef) where TTask : IRuntimeTask
        {
            return runtime.CreateRuntimeTask<TTask>(implementationRef, ScopeContext.GetDefaultScope());
        }

        /// <summary>
        /// Creates and initializes a new IRuntimeTask implementation. 
        /// </summary>
        /// <param name="implementationRef"></param>
        /// <param name="scope">Deprecated, no longer in use. Defined on binding</param>
        public static TTask CreateRuntimeTask<TTask>(this IRuntime runtime, string implementationRef, Scope scope) where TTask : IRuntimeTask
        {
            var instance = Resolver.Activate<TTask>(implementationRef, t => InitializeTask(t, runtime));
            return instance;
        }

        private static void InitializeTask(IRuntimeTask task, IRuntime runtime)
        {
            task.SetExternalState(ref runtime)
                .SetInvokerStateStorage(runtime.GetStateStorageContainer());
        }
    }
}