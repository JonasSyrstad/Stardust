using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Stardust.Interstellar;
using Stardust.Interstellar.Messaging;
using Stardust.Nucleus;
using Stardust.Nucleus.ContextProviders;
using Stardust.Particles;
using Utilities = Stardust.Interstellar.Utilities.Utilities;

namespace Stardust.Core.Wcf
{
    public class ClientHeaderInspector : IParameterInspector, IClientMessageInspector
    {

        private static IScopeProvider GetScope()
        {
            return ContainerFactory.Current.GetProvider(Scope.Context);
        }

        public object BeforeCall(string operationName, object[] inputs)
        {
            var runtime = RuntimeFactory.Current;
            try
            {

                if (inputs.IsNull()) return null;
                bool isStardustRequest = false;
                foreach (var input in inputs)
                {
                    if (!(input is IRequestBase) && !(input is RequestHeader)) continue;
                    var message = input as IRequestBase;
                    var header = input as RequestHeader;
                    if (header.IsNull())
                        header = message.RequestHeader;
                    if (header.IsNull())
                    {
                        message.RequestHeader = header = new RequestHeader();
                    }
                    CreateRequestHeader(operationName, header);
                    isStardustRequest = true;
                }
                if (isStardustRequest)
                {
                    var header = CreateRequestHeader(operationName, new RequestHeader());
                    GetScope().Bind(typeof(RequestHeader), header);
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            return runtime;
        }

        private RequestHeader CreateRequestHeader(string operationName, RequestHeader requestHeader)
        {
            requestHeader.MethodName = operationName;
            requestHeader.RuntimeInstance = RuntimeFactory.GetInstanceId().ToString();
            requestHeader.ServerIdentity = Environment.MachineName;
            SetMessageValuesIfNotSetInClient(requestHeader, RuntimeFactory.Current);
            return requestHeader;
        }

        private static void SetMessageValuesIfNotSetInClient(RequestHeader message, IRuntime runtime)
        {
            if (runtime == null) return;
            if (message.MessageId.IsNullOrWhiteSpace())
                message.MessageId = Guid.NewGuid().ToString();
            if (message.Environment.IsNullOrWhiteSpace())
                message.Environment = runtime.Environment;
            if (message.ServiceName.IsNullOrWhiteSpace())
                message.ServiceName = runtime.ServiceName;
            if (message.ConfigSet.IsNullOrWhiteSpace())
                message.ConfigSet = Utilities.GetConfigSetName();
            if (message.TimeStamp == null)
                message.TimeStamp = DateTime.UtcNow;
            if (runtime.RequestContext.IsInstance() && runtime.RequestContext.RequestHeader.IsInstance())
            {
                message.ReferingMessageId = runtime.RequestContext.RequestHeader.MessageId;

            }
            string supportCode;
            if (runtime.TryGetSupportCode(out supportCode)) message.SupportCode = supportCode;
            else
            {
                if (runtime.RequestContext.IsInstance() && runtime.RequestContext.RequestHeader.IsInstance())
                {
                    message.SupportCode = runtime.RequestContext.RequestHeader.SupportCode;
                }
            }
        }

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            try
            {
                var runtime = correlationState as IRuntime;
                if (runtime == null) runtime = RuntimeFactory.Current;
                if (returnValue is IResponseBase)
                {
                    try
                    {
                        var rb = returnValue as IResponseBase;
                        if (rb.ResponseHeader.IsNull() || rb.ResponseHeader.CallStack.IsNull()) return;
                        if (runtime.GetTracer() != null && !runtime.GetTracer().IsDisposed)
                            runtime.GetTracer().AppendCallstack(rb.ResponseHeader.CallStack);
                        return;
                    }
                    catch
                    {
                    }
                }
                foreach (var output in outputs)
                {
                    if (output is ResponseHeader)
                    {
                        var rb = output as ResponseHeader;
                        if (rb.IsNull() || rb.CallStack.IsNull()) return;
                        if (runtime.GetTracer() != null && !runtime.GetTracer().IsDisposed)
                            runtime.GetTracer().AppendCallstack(rb.CallStack);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            try
            {
                if (request.Version.Envelope == EnvelopeVersion.None)
                {
                    return RuntimeFactory.Current;
                }
                var header = GetScope().Resolve(typeof(RequestHeader)) as RequestHeader;
                request.Headers.Add(header.IsInstance()
                    ? MessageHeader.CreateHeader("RequestHeader", RequestHeader.NS, header)
                    : MessageHeader.CreateHeader("HeaderInfoIncluded", "http://stardustframework.com/interstellar/messaging/headers/included", "RequestHeaderIncludedInDataOrMessageContract"));
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            return RuntimeFactory.Current;
        }

        private static void SetConnectivityErrorFlag(Exception ex, IRuntime runtime)
        {
            if (runtime == null) return;
            runtime.GetStateStorageContainer().TryAddStorageItem(ex.Message, "connectionErrorMessage");
            runtime.GetStateStorageContainer().TryAddStorageItem(true, "connectionError");
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            if (reply.Version.Envelope == EnvelopeVersion.None)
            {
                return;
            }
            var runtime = correlationState as IRuntime;
            SetCommunicationStatusFlags(ref reply, runtime);
            try
            {
                if (reply.Headers.All(messageHeader => messageHeader.Name != "ResponseHeader")) return;
                try
                {
                    var header = reply.Headers.GetHeader<ResponseHeader>("ResponseHeader", ResponseHeader.NS);
                    if (header.CallStack.IsInstance() && runtime.GetTracer().IsInstance())
                    {
                        if (runtime.GetTracer() != null && !runtime.GetTracer().IsDisposed)
                            runtime.GetTracer().AppendCallstack(header.CallStack);
                    }
                }
                catch (Exception ex)
                {
                    ex.Log();
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        private static void SetCommunicationStatusFlags(ref Message reply, IRuntime runtime)
        {
            if (!reply.IsFault) return;
            try
            {
                var buffer = reply.CreateBufferedCopy(int.MaxValue);
                var copy = buffer.CreateMessage();
                if (!copy.IsFault) return;
                var fault = MessageFault.CreateFault(copy, int.MaxValue);
                var error = fault.Reason.GetMatchingTranslation().Text;
                reply = buffer.CreateMessage();
                var exception = new CommunicationException(error);
                SetConnectivityErrorFlag(exception, runtime);
            }
            catch (Exception ex)
            {
                SetConnectivityErrorFlag(ex, runtime);
            }
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }
    }
}