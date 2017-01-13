using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Stardust.Core.Wcf;
using Stardust.Particles;

namespace Stardust.Core
{
    public sealed class ThreadSynchronizationContext : SynchronizationContext, IStardustContext
    {
        public static ThreadSynchronizationContext CreateBlockingContext()
        {
            var context = Current as ThreadSynchronizationContext;
            if (context != null)
            {
                return new ThreadSynchronizationContext
                           {
                               isSingleThread = true,
                               Id = context.Id,
                               OldContext = context,
                               IsSyncRoot = false,
                               StardustThreadId = context.StardustThreadId,
                               StateContainer = context.StateContainer
                           };
            }
            return new ThreadSynchronizationContext { isSingleThread = true };
        }

        public void RunOnCurrentThread()
        {

            KeyValuePair<SendOrPostCallback, object> workItem;

            while (Queue.TryTake(out workItem, Timeout.Infinite))

                workItem.Key(workItem.Value);

        }

        public void Complete() { Queue.CompleteAdding(); }


        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> Queue = new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

        public static IStardustContext CurrentContext
        {
            get { return Current as IStardustContext; }
        }

        internal static IStardustContext BeginContext(Guid id)
        {
            return BeginContext(id, true);
        }

        private static ThreadSynchronizationContext BeginContext(Guid id, bool asRoot)
        {
            var context = new ThreadSynchronizationContext
            {
                OldContext = Current,
                Id = new Guid(id.ToString()),
                IsSyncRoot = asRoot
            };
            SetSynchronizationContext(context);
            return context;
        }

        private bool IsSyncRoot { get; set; }

        public Guid Id { get; private set; }

        private SynchronizationContext OldContext { get; set; }

        public override void Post(SendOrPostCallback d, object state)
        {
            if (isSingleThread) Queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
            else
                base.Post(chainedState => CallWithOperationContext(d, state), state);

        }

        public override void Send(SendOrPostCallback d, object state)
        {
            DebugMessage("sending msg");
            if (CanSendInline)
            {
                d(state);
            }
            else
            {
                using (var handledEvent = new ManualResetEvent(false))
                {
                    Post(SendOrPostCallback_BlockingWrapper, Tuple.Create(d, state, handledEvent));
                    handledEvent.WaitOne();
                }
            }
        }
        private static void SendOrPostCallback_BlockingWrapper(object state)
        {
            var innerCallback = (state as Tuple<SendOrPostCallback, object, ManualResetEvent>);
            try
            {
                innerCallback.Item1(innerCallback.Item2);
            }
            finally
            {
                innerCallback.Item3.Set();
            }
        }

        private bool IsDisposed { get; set; }

        public override SynchronizationContext CreateCopy()
        {
            if (DoLogging)
            {
                DebugMessage("Creating copy", Current, this);
            }
            return this;
        }

        private static bool DoLogging
        {
            get { return ConfigurationManagerHelper.GetValueOnKey("stardust.Debug") == "true"; }
        }

        public override string ToString()
        {
            return string.Format("ContextId {1} thread {0}", StardustThreadId, ContextId);
        }

        void CallWithOperationContext(SendOrPostCallback chainedCallback, object chainedState)
        {
            var old = Current;
            SetSynchronizationContext(this);
            try
            {
                chainedCallback(chainedState);
            }
            catch (Exception ex)
            {
                Logging.Exception(ex);
            }
            finally
            {
                SetSynchronizationContext(old);
            }
        }

        private bool CanSendInline
        {
            get
            {
                return Thread.CurrentThread.IsThreadPoolThread;
            }
        }


        public void Dispose()
        {
            if (!IsSyncRoot || SignaledSubscribers)
            {
                if (!IsDisposed)
                {
                    DebugMessage("disposing {0}", StardustThreadId);
                    Queue.Dispose();
                    GC.SuppressFinalize(this);
                    IsDisposed = true;

                }
                return;
            }
            DebugMessage("Cleaning context");
            SetSynchronizationContext(OldContext);
            SignalAndClean();
            if (!IsDisposed)
            {
                DebugMessage("disposing {0}", StardustThreadId);
                Queue.Dispose();
                GC.SuppressFinalize(this);
                IsDisposed = true;
                
            }
            if (!DoLogging) return;
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    var st1 = new StackTrace(new StackFrame(i, true));
                    DebugMessage("Stack{1}: {0}", st1.ToString(), i);
                }
            }
            catch (Exception)
            {
            }
        }

        private static int globalLogSequence;
        private int localLogSequence;

        private bool isSingleThread;

        private void DebugMessage(string format, params object[] args)
        {
            if (!DoLogging) return;
            lock (this)
            {
                globalLogSequence++;
                localLogSequence++;
            }
            var formatString = string.Format("[{0}/{1}] {2} [{3}]", globalLogSequence, localLogSequence, format, Id);
            Logging.DebugMessage(formatString, args);
        }

        private void Clean()
        {
            try
            {
                StateContainer.Dispose();
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        public static IStardustContext BeginContext()
        {
            return BeginContext(Guid.NewGuid(), true);
        }

        public Guid ContextId { get { return Id; } }

        private void SignalAndClean()
        {
            if (Disposing != null)
                Disposing(this, new EventArgs());
            SignaledSubscribers = true;
            Clean();
        }

        private bool SignaledSubscribers { get; set; }

        private ThreadSynchronizationContext()
        {
            StardustThreadId = Guid.NewGuid();
            StateContainer = new StardustContextProvider();
            DebugMessage("creating {0}", StardustThreadId);
        }
        
        private Guid StardustThreadId { get; set; }

        internal StardustContextProvider StateContainer { get; private set; }

        public event EventHandler<EventArgs> Disposing;

        internal void SetOldContext(SynchronizationContext old)
        {
            OldContext = old;
        }
    }
}