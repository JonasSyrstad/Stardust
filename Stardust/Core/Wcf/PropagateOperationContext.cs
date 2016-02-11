using System;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.Threading;
using Stardust.Particles;

namespace Stardust.Core.Wcf
{
    public static class PropagateOperationContext
    {
        /// <summary>
        ///     Propagate the operation context across thread boundaries (eg. for async / await).
        /// </summary>
        /// <param name="operationContext">
        ///     The operation context to propagate.
        /// </param>
        /// <returns>
        ///     An <see cref="IDisposable"/> implementation that restores the previous synchronisation context when disposed.
        /// </returns>
        /// <remarks>
        ///     Also sets the operation context, as a convenience, for the calling thread.
        ///     This is usually what you want, in async / await scenarios.
        /// </remarks>
        public static IDisposable Propagate(this OperationContext operationContext)
        {
            if (operationContext == null)
                return new NullContext();
            //RestoreStateContainerOnNewThread(operationContext);
            return
                new ContextScope(
                    new OperationContextPreservingSynchronizationContext(
                        operationContext
                    )
                );
        }

        /// <summary>
        ///     Use the operation context as the current operation context.
        /// </summary>
        /// <param name="operationContext">
        ///     The operation context to use.
        /// </param>
        /// <returns>
        ///     An <see cref="IDisposable"/> implementation that restores the operation context when disposed.
        /// </returns>
        /// <remarks>
        ///     Also sets the operation context, as a convenience, for the calling thread.
        ///     This is usually what you want, in async / await scenarios.
        /// </remarks>
        public static IDisposable Use(this OperationContext operationContext)
        {
            if (operationContext == null)
                return new NullContext();
            return new OperationContextScope(operationContext);
        }

        /// <summary>
        ///     Use the operation context as the current operation context, and propagate it across thread boundaries (eg. for async / await).
        /// </summary>
        /// <param name="operationContext">
        ///     The operation context to use / propagate.
        /// </param>
        /// <returns>
        ///     An <see cref="IDisposable"/> implementation that restores the previous synchronisation and operation contexts when disposed.
        /// </returns>
        public static IDisposable UseAndPropagate(this OperationContext operationContext)
        {
            if (operationContext == null)
                return new NullContext();

            
            return new ContextScope(new OperationContextPreservingSynchronizationContext(operationContext), operationContext);
        }

        
    }

    public class NullContext : IDisposable
    {
        public NullContext()
        {
            Logging.DebugMessage("NullContextCreated.... But why????");
        }

        public void Dispose()
        {
        }
    }


    /// <summary>
    ///     A custom synchronisation context that propagates the operation context across threads.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "We don't actually want to dispose the operation context scope because it may wind up being disposed on a different thread than the one that created it.")]
    internal class OperationContextPreservingSynchronizationContext : SynchronizationContext
    {
        /// <summary>
        ///     The operation context to propagate.
        /// </summary>
        readonly OperationContext _operationContext;

        /// <summary>
        ///     Object used for locking the live scope.
        /// </summary>
        readonly object _scopeLock = new object();

        /// <summary>
        ///     Our live operation context scope.
        /// </summary>
        OperationContextScope _operationContextScope;


        /// <summary>
        ///     Create a new operation-context-preserving synchronization context.
        /// </summary>
        /// <param name="operationContext">
        ///     The operation context to propagate.
        /// </param>
        public OperationContextPreservingSynchronizationContext(OperationContext operationContext)
        {
            if (operationContext == null)
                throw new ArgumentNullException("operationContext");

            _operationContext = operationContext;
        }

        /// <summary>
        ///     Create a copy of the synchronisation context.
        /// </summary>
        /// <returns>
        ///     The new synchronisation context.
        /// </returns>
        public override SynchronizationContext CreateCopy()
        {
            return new OperationContextPreservingSynchronizationContext(_operationContext);
        }

        /// <summary>
        ///     Dispatch a synchronous message to the synchronization context.
        /// </summary>
        /// <param name="callback">
        ///     The <see cref="SendOrPostCallback"/> delegate to call.
        /// </param>
        /// <param name="state">
        ///     The state object passed to the delegate.
        /// </param>
        /// <exception cref="NotSupportedException">
        ///     The method was called in a Windows Store app. The implementation of <see cref="SynchronizationContext"/> for Windows Store apps does not support the <see cref="SynchronizationContext.Send"/> method.
        /// </exception>
        public override void Send(SendOrPostCallback callback, object state)
        {
            Logging.DebugMessage("Send operation context!");
            base.Send(
                chainedState =>
                    CallWithOperationContext(callback, state),
                state
            );
        }

        /// <summary>
        ///     Dispatch an asynchronous message to the synchronization context.
        /// </summary>
        /// <param name="callback">
        ///     The <see cref="SendOrPostCallback"/> delegate to call in the synchronisation context.
        /// </param>
        /// <param name="state">
        ///     The state object passed to the delegate.
        /// </param>
        public override void Post(SendOrPostCallback callback, object state)
        {

            Logging.DebugMessage("Post operation context!");
            base.Post(
                chainedState =>
                    CallWithOperationContext(callback, state),
                state
            );
        }

        /// <summary>
        ///     Push a new operation context scope onto the scope stack, if required.
        /// </summary>
        /// <remarks>
        ///     <c>true</c>, if a new operation context scope was created, otherwise, <c>false</c>.
        /// </remarks>
        bool PushOperationContextScopeIfRequired()
        {
            if (OperationContext.Current == _operationContext) return false;
            lock (_scopeLock)
            {
                ReleaseOperationContextScopeIfRequired();
                _operationContextScope = new OperationContextScope(_operationContext);
                return true;
            }
        }

        

        /// <summary>
        ///     Release the current operation context scope generated by the synchronisation context (if it exists).
        /// </summary>
        void ReleaseOperationContextScopeIfRequired()
        {
            if (_operationContextScope == null)
            {
                lock (_scopeLock)
                {
                    if (_operationContextScope != null)
                    {
                        _operationContextScope.Dispose();
                        _operationContextScope = null;
                    }
                }
            }
        }

        /// <summary>
        ///     Call a callback delegate with a the operation context set.
        /// </summary>
        /// <param name="chainedCallback">
        ///     The chained delegate to call.
        /// </param>
        /// <param name="chainedState">
        ///     The callback state, if any.
        /// </param>
        void CallWithOperationContext(SendOrPostCallback chainedCallback, object chainedState)
        {
            if (chainedCallback == null)
                throw new ArgumentNullException("chainedCallback");

            bool pushedNewScope = PushOperationContextScopeIfRequired();
            try
            {
                using (OperationContext.Current.UseAndPropagate())
                {
                    chainedCallback(chainedState);
                }
            }
            finally
            {
                if (pushedNewScope)
                    ReleaseOperationContextScopeIfRequired();
            }
        }
    }
    internal class ContextScope : IDisposable
    {
        /// <summary>
        ///     The new synchronisation context.
        /// </summary>
        readonly OperationContextPreservingSynchronizationContext _newContext;

        /// <summary>
        ///     The old synchronisation context.
        /// </summary>
        readonly SynchronizationContext _oldContext;

        /// <summary>
        ///     The operation context scope (if any) that was already set for the calling thread when the scope was created.
        /// </summary>
        readonly OperationContext _preexistingContext;

        /// <summary>
        ///     Have we been disposed?
        /// </summary>
        bool _disposed;

        /// <summary>
        ///     Create a new context scope.
        /// </summary>
        /// <param name="newContext">
        ///     The new context.
        /// </param>
        /// <param name="setAsCurrentForCallingThread">
        ///     The operation context (if any) to set as the current context for the calling thread.
        ///     If <c>null</c>, no operation context will be set for the calling thread.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "We don't dispose the context; it will be replaced when another context is created.")]
        public ContextScope(OperationContextPreservingSynchronizationContext newContext, OperationContext setAsCurrentForCallingThread = null)
        {
            if (newContext == null)
                throw new ArgumentNullException("newContext");

            _newContext = newContext;
            _oldContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(_newContext);

            if (setAsCurrentForCallingThread != null)
            {
                // Save it so we can restore it when we're disposed.
                _preexistingContext = OperationContext.Current;


                // Set-and-forget.
                new OperationContextScope(setAsCurrentForCallingThread);
            }
        }

        /// <summary>
        ///     Release the scope.
        /// </summary>
        /// <remarks>
        ///     We don't dispose the calling thread's synchronisation scope; we expect that it would already have gone out of scope due to async / await state machine behaviour.
        /// </remarks>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "We don't dispose the context; it will be replaced when another context is created.")]
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true; // Whatever happens, don't attempt this more than once.

                SynchronizationContext.SetSynchronizationContext(_oldContext);



                // Restore the existing operation context, if one was present when the scope was created.
                if (_preexistingContext != null)
                    new OperationContextScope(_preexistingContext);



                GC.SuppressFinalize(this);
            }
        }
    }

}

