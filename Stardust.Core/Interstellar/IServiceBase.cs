using System;
using System.ComponentModel;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Trace;

namespace Stardust.Interstellar
{
    /// <summary>
    /// The interception point for initializing the service and
    /// <see cref="IRuntime" /> . The <see langword="interface"/> members are
    /// called from within the framework it self. You do not need to call any of
    /// the members unless you <see langword="override"/>
    /// DoManualRuntimeInitialization and return true. This indicates that
    /// initialization and service teardown will be handled by the service. Most
    /// commonly this is only done for your logging service, which would then
    /// call it self recursively
    /// </summary>
    public interface IServiceBase
    {
        /// <summary>
        /// The shared <see cref="IRuntime"/> instance for the service request
        /// </summary>
        IRuntime Runtime { get; }

        /// <summary>
        /// Initializes the service and <see cref="IRuntime"/> 
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        ITracer Initialize(string methodName = null);

        /// <summary>
        /// Initializes the service and <see cref="IRuntime"/> 
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void Initialize(IRequestBase message, string methodName = null);

        /// <summary>
        /// Initializes the service and <see cref="IRuntime"/> 
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void Initialize(string environment, string serviceName, string methodName = null);

        /// <summary>
        /// Wraps up the service request an logs the execution and any exceptions
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void TearDown();

        /// <summary>
        /// Wraps up the service request an logs the execution and any exceptions
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        T TearDown<T>(T message) where T : IResponseBase;

        /// <summary>
        /// Wraps up the service request an logs the execution and any exceptions
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Exception TearDown(Exception exception);

        /// <summary>
        /// Returns true if the framework should skip initialization
        /// </summary>
        bool DoManualRuntimeInitialization { get; }

    }
}