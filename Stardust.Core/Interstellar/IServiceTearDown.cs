using System;
using System.ComponentModel;
using Stardust.Interstellar.Messaging;

namespace Stardust.Interstellar
{
    /// <summary>
    /// Provides an interface for adding logging to service tear-down
    /// </summary>
    public interface IServiceTearDown
    {
        /// <summary>
        /// Wraps up service execution and logs the <paramref name="exception"/>
        /// </summary>
        /// <param name="runtime">The <see cref="IRuntime"/> instance for the service request</param>
        /// <param name="exception"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void TearDown(IRuntime runtime, Exception exception);

        /// <summary>
        /// Wraps up service execution and logs the payload provided
        /// </summary>
        /// <param name="runtime">The <see cref="IRuntime"/> instance for the service request</param>
        /// <param name="payload"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void TearDown(IRuntime runtime, string payload);

        /// <summary>
        /// Wraps up service execution and logs the <paramref name="message"/>
        /// </summary>
        /// <param name="runtime">
        /// The <see cref="IRuntime" /> instance for the service request
        /// </param>
        /// <param name="message"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        T TearDown<T>(IRuntime runtime, T message) where T : IResponseBase;
    }
}