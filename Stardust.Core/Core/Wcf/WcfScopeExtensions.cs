using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Stardust.Interstellar;
using Stardust.Interstellar.Trace;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Core.Wcf
{

    static class PeriodicTask
    {
        public static void Run(Action<object, CancellationToken> doWork, object taskState, TimeSpan period, CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                do
                {
                    await Task.Delay(period, cancellationToken).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                    doWork(taskState, cancellationToken);
                } while (true);
            });
        }
    }
    public static class RequestResponseScopefactory
    {
        public static IStardustContext CreateScope()
        {
            return ContextScopeExtensions.CreateScope();
        }
    }

    public static class ContextScopeExtensions
    {
        static ContextScopeExtensions()
        {
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.enableOlmCleaningTask", false))
                PeriodicTask.Run(CleanStateCache, null, TimeSpan.FromMinutes(10), CancellationToken.None);
        }

        private static void CleanStateCache(object o, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var c in StateStorage.Where(i => i.Value.Created < DateTime.UtcNow.AddHours(-3)).ToArray())
                {
                    StardustContextProvider deprecatedItem;
                    lock (StateStorage)
                        StateStorage.TryRemove(c.Key, out deprecatedItem);
                }
            }
            catch
            {
            }
        }
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
            ctx.SetDisconnectorAction(CurrentContextOnOperationCompleted);
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

        private static bool DoLogging { get; } = ConfigurationManagerHelper.GetValueOnKey("stardust.Debug") == "true";

        private static bool DoLoggingLight { get; } = ConfigurationManagerHelper.GetValueOnKey("stardust.LightDebug") == "true";

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
                instance?.TryDispose();
            }
        }

        public static void ClearContext(this IStardustContext currentContext)
        {
            var container = GetContainer(currentContext);
            lock (container)
            {
                container.Clear();
                GetStardustContextProvider(currentContext).DisposeList?.AsParallel().ForAll(i=>i?.TryDispose());
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
                    {
                        if (DoLoggingLight) Logging.DebugMessage($"Unable to remove item {currentContext.ContextId}");
                        return;
                    }
                }
                if (item.DisposeList != null)
                {
                    if (DoLoggingLight) Logging.DebugMessage($"Disposing {item.DisposeList.Count} items for context {currentContext.ContextId}");
                    foreach (var disposable in item.DisposeList)
                    {
                        if (DoLoggingLight) Logging.DebugMessage($"Disposing {disposable?.GetType().Name}");
                        disposable.TryDispose();
                    }
                }
                if (DoLoggingLight) Logging.DebugMessage($"Disposing clearing disposable list");
                item?.DisposeList?.Clear();
                if (DoLoggingLight) Logging.DebugMessage($"Disposing clearing container");
                item?.Dispose();
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
            try
            {
                var threadLocker = context.GetItemFromContext(typeof(ManualResetEvent).FullName) as ManualResetEvent;
                threadLocker?.WaitOne();
            }
            catch (Exception ex)
            {
                if (DoLoggingLight) ex.Log("WTF?!?");
            }
        }

        internal static void RegisterForDispose(this IStardustContext currentContext, IDisposable instance)
        {
            try
            {

                if (DoLoggingLight) Logging.DebugMessage($"Register disposable item {instance.GetType().FullName}");
                var container = GetStardustContextProvider(currentContext);
                lock (container)
                {
                    if (DoLoggingLight) Logging.DebugMessage($"before insert: Dispose list size {container.DisposeList?.Count}");
                    container.DisposeList?.Add(instance);
                    if (DoLoggingLight) Logging.DebugMessage($"after insert: Dispose list size {container.DisposeList?.Count}");
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        private static void CurrentContextOnOperationCompleted(object sender)
        {
            IStardustContext context = null;
            try
            {
                context = (IStardustContext)sender;
                WaitOperationRelease(context);
                DisposeContext(context);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            finally
            {
                if (context != null)
                {
                    if (DoLoggingLight) Logging.DebugMessage($"Disconnecting event handler {context.ContextId}");
                    context.ClearDisposeActoion();
                }
            }
        }
    }
}