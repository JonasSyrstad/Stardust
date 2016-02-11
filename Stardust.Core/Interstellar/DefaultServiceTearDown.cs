using System;
using System.Diagnostics;
using System.Threading;
using Stardust.Interstellar.Messaging;
using Stardust.Particles;
using Stardust.Particles.Xml;

namespace Stardust.Interstellar
{
    /// <summary>
    /// The default implementation of <see cref="IServiceTearDown"/>. Utilized
    /// ILogging to create log entries. 
    /// </summary>
    /// <remarks>Note that the logging performed within this implementation does
    /// not ensure entry order as it does the logging async. Implement your own
    /// if you need better performance, tracing and ensure ordering. In the
    /// sample application this has proven to be a bit slow and unreliable
    /// regarding performance. It is recommended to use MongoDB or an other high
    /// performance storage.</remarks>
    public class DefaultServiceTearDown : IServiceTearDown
    {
        private const string CallStackNamespace = "http://stardustframework.com/default/CallStack/";

        void IServiceTearDown.TearDown(IRuntime runtime, Exception exception)
        {
            ThreadPool.QueueUserWorkItem(task => Logging.Exception(exception, string.Format("Exception thrown in runtime instance: {0}", runtime.InstanceId)));
            LogCallStack(runtime);
        }

        void IServiceTearDown.TearDown(IRuntime runtime, string payload)
        {
            ThreadPool.QueueUserWorkItem(task => Logging.DebugMessage(payload));
            LogCallStack(runtime);
        }

        T IServiceTearDown.TearDown<T>(IRuntime runtime, T message)
        {
            message = AddMetricsToMessage(runtime, message);
            CalculateExecutionTime(runtime, message);
            LogCallStack(runtime);
            ThreadPool.QueueUserWorkItem(task => Logging.DebugMessage(string.Format("Runtime instance {0} completed in {1}ms", runtime.InstanceId, runtime.CallStack.ExecutionTime)));
            return message;
        }

        private static void LogCallStack(IRuntime runtime)
        {
            var CallStack = runtime.CallStack;
            ThreadPool.QueueUserWorkItem(task =>
            {
                var data = CallStack.Serialize(CallStackNamespace, true);


                Logging.DebugMessage(data, EventLogEntryType.SuccessAudit);
            });
        }


        private static T AddMetricsToMessage<T>(IRuntime runtime, T message) where T : IResponseBase
        {
            if(message.ResponseHeader.IsNull())message.ResponseHeader=new ResponseHeader();
            if (message.ResponseHeader.MessageId.IsNullOrWhiteSpace())
                message.ResponseHeader.MessageId = Guid.NewGuid().ToString();
            if (message.ResponseHeader.TimeStamp == null)
                message.ResponseHeader.TimeStamp = DateTime.UtcNow;
            message.ResponseHeader.RuntimeInstance = runtime.InstanceId.ToString();
            message.ResponseHeader.OriginalRuntimeInstance = runtime.RequestContext.IsInstance() ? runtime.RequestContext.RequestHeader.RuntimeInstance : "";
            if (message.ResponseHeader.ReferingMessageId.IsNullOrWhiteSpace())
                message.ResponseHeader.ReferingMessageId = runtime.RequestContext.IsInstance() ? runtime.RequestContext.RequestHeader.MessageId : "";
            AppendCallStack(runtime, message);
            return message;
        }

        private static void AppendCallStack<T>(IRuntime runtime, T message) where T : IResponseBase
        {
            if (runtime.Context.GetEnvironmentConfiguration().GetConfigParameter("disableCallStackPropagation") != "true")
                message.ResponseHeader.CallStack = runtime.CallStack;
        }

        internal static void CalculateExecutionTime<T>(IRuntime runtime, T message) where T : IResponseBase
        {
            if (runtime.CallStack.EndTimeStamp.HasValue)
                message.ResponseHeader.ExecutionTime = (long)(runtime.CallStack.EndTimeStamp.Value - runtime.CallStack.TimeStamp).TotalMilliseconds;
        }
    }
}