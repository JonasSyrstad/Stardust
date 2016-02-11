using System;
using System.Threading;
using System.Threading.Tasks;
using Stardust.Core.Wcf;

namespace Stardust.Core
{
    public class AsyncPump
    {
        /// <summary>
        /// Shedules a task to run on the current thread. Use in console apps powershell and other async-> sync scenarios
        /// </summary>
        /// <param name="func"></param>
        public static void Run(Func<Task> func)
        {
            var prevCtx = SynchronizationContext.Current;
            try
            {
                var syncCtx = ThreadSynchronizationContext.CreateBlockingContext();
                SynchronizationContext.SetSynchronizationContext(syncCtx);
                var t = func();
                t.ContinueWith(delegate { syncCtx.Complete(); }, TaskScheduler.Default);
                syncCtx.RunOnCurrentThread();
                t.GetAwaiter().GetResult();
            }
            finally { SynchronizationContext.SetSynchronizationContext(prevCtx); }
        }
    }
}