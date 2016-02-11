using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Stardust.Interstellar;
using Stardust.Interstellar.Trace;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Core.Wcf
{
    public static class ContextScopeExtensions
    {
        private const string stardustcontextid = "stardustContextId";


        internal static IStardustContext CreateScope()
        {
            var id = Guid.NewGuid();
            var ctx = ThreadSynchronizationContext.BeginContext(id);

           
            ContainerFactory.Current.Bind(typeof(Guid?), id, Scope.Context);
            var runtime = RuntimeFactory.CreateRuntime(Scope.PerRequest);
            ContainerFactory.Current.Bind(typeof(IRuntime), runtime, Scope.Context);
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

        

        private static ConcurrentDictionary<string, object> GetContainer(IStardustContext currentContext)
        {
            var container = GetStardustContextProvider(currentContext);
            return container.Container;
        }

        private static StardustContextProvider GetStardustContextProvider(IStardustContext currentContext)
        {

            try
            {
                return ((ThreadSynchronizationContext)currentContext).StateContainer;
            }
            catch (Exception ex)
            {
                Logging.Exception(ex);
                //if (DoLogging)
                {
                    try
                    {
                        //if (DoLogging)
                        {
                            for (var i = 0; i < 100; i++)
                            {
                                var st1 = new StackTrace(new StackFrame(i, true));
                                if(st1.ToString().ContainsCharacters())
                                Logging.DebugMessage("[ext] Stack{1}: {0}  [{2}]", st1.ToString(), i, currentContext.ContextId);
                                else break;

                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
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
            var context = (IStardustContext)sender;
            WaitOperationRelease(context);
            context.Disposing -= CurrentContextOnOperationCompleted;
        }
    }
}