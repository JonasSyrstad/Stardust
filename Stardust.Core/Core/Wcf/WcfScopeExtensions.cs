using System;
using System.Collections.Concurrent;
using System.Threading;
using Stardust.Interstellar;
using Stardust.Interstellar.Trace;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Core.Wcf
{
    public static class RequestResponseScopefactory
    {
        public static IStardustContext CreateScope()
        {
            return ContextScopeExtensions.CreateScope();
        }
    }

    public static class ContextScopeExtensions
    {
        public static int GetActiveScopes() => StateStorage.Count;
        private static readonly ConcurrentDictionary<string, StardustContextProvider> StateStorage = new ConcurrentDictionary<string, StardustContextProvider>();

        internal static IStardustContext CreateScope()
        {
            var id = Guid.NewGuid();
            var ctx = ThreadSynchronizationContext.BeginContext(id);

            lock (StateStorage)
            {
                if (!StateStorage.TryAdd(id.ToString(), new StardustContextProvider())) Logging.DebugMessage("Unable to initialize context");
                if (DoLogging) Logging.DebugMessage("creating scope storage for {0}", id);
            }
            ContainerFactory.Current.Bind(typeof(Guid?), id, Scope.Context);
            var runtime = RuntimeFactory.CreateRuntime(Scope.PerRequest);
            ContainerFactory.Current.Bind(typeof(IRuntime), runtime, Scope.Context);
            ContainerFactory.Current.Bind(runtime.GetType(), runtime, Scope.Context);
            ContainerFactory.Current.Bind(typeof(InvokationMarker), new InvokationMarker(DateTime.UtcNow), Scope.Context);
            ContainerFactory.Current.Bind(typeof(TraceHandler), new TraceHandler(), Scope.Context);
            ctx.Disposing += CurrentContextOnOperationCompleted;
            return ctx;
        }

        internal class InvokationMarker
        {
            private readonly DateTime timestamp;

            public InvokationMarker(DateTime timestamp)
            {
                this.timestamp = timestamp;
            }
        }

        private static bool DoLogging
        {
            get { return ConfigurationManagerHelper.GetValueOnKey("stardust.Debug") == "true"; }
        }

        public static object GetItemFromContext(this IStardustContext currentContext, string key)
        {
            var container = GetContainer(currentContext);
            lock (container)
            {
                object instance;
                if (container.TryGetValue(key, out instance)) return instance;
                if (DoLogging && key != "Stardust.Nucleus.ContextProviders.IExtendedScopeProvider") Logging.DebugMessage("Item with key {0} was not found in context {1}", key, currentContext.ContextId);
                return null;
            }
        }

        public static void SetItemInContext(this IStardustContext currentContext, string key, object item)
        {
            var container = GetContainer(currentContext);
            lock (container)
            {
                if (DoLogging) Logging.DebugMessage("inserting {0}", key);
                if (container.ContainsKey(key))
                {
                    object oldValue;
                    container.TryRemove(key, out oldValue);
                }
                if (!container.TryAdd(key, item)) Logging.DebugMessage("inserting item {0} failed", key);
            }
        }


        public static void RemoveItemFromContext(this IStardustContext currentContext, string key)
        {
            var container = GetContainer(currentContext);
            lock (container)
            {
                object instance;
                if (!container.TryRemove(key, out instance)) Logging.DebugMessage("failed to remove item '{0}'", key);
            }
        }

        public static void ClearContext(this IStardustContext currentContext)
        {
            var container = GetContainer(currentContext);
            lock (container)
            {
                container.Clear();
            }
        }

        internal static void DisposeContext(this IStardustContext currentContext)
        {
            try
            {
                StardustContextProvider item;
                int counter = 0;
                while (!StateStorage.TryRemove(currentContext.ContextId.ToString(), out item))
                {
                    Thread.Sleep(10);
                    counter++;
                    if (counter > 10)
                        break;
                }
                foreach (var disposable in item.DisposeList)
                {
                    disposable.TryDispose();
                }
                item.DisposeList.Clear();
                item.Dispose();
            }
            catch
            {
            }
        }

        private static ConcurrentDictionary<string, object> GetContainer(IStardustContext currentContext)
        {
            var container = GetStardustContextProvider(currentContext);
            return container.Container;
        }

        private static StardustContextProvider GetStardustContextProvider(IStardustContext currentContext)
        {
            try
            {
                if (currentContext == null)
                {
                    Logging.DebugMessage("WTF??");
                    currentContext = ThreadSynchronizationContext.BeginContext(Guid.NewGuid());
                }
                return ((ThreadSynchronizationContext)currentContext).StateContainer;
            }
            catch (Exception ex)
            {
                Logging.Exception(ex);
                throw;
            }
        }

        private static void WaitOperationRelease(IStardustContext context)
        {
            var threadLocker = context.GetItemFromContext(typeof(ManualResetEvent).FullName) as ManualResetEvent;
            if (threadLocker.IsInstance())
                threadLocker.WaitOne();
        }

        internal static void RegisterForDispose(this IStardustContext currentContext, IDisposable instance)
        {
            var container = GetStardustContextProvider(currentContext);
            container.DisposeList.Add(instance);
        }

        private static void CurrentContextOnOperationCompleted(object sender, EventArgs eventArgs)
        {
            IStardustContext context=null;
            try
            {
                context = (IStardustContext)sender;
                WaitOperationRelease(context);
                DisposeContext(context);
            }
            finally
            {
                if (context != null) context.Disposing -= CurrentContextOnOperationCompleted;
            }
        }
    }
}