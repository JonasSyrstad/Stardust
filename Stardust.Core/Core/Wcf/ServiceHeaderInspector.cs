using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;
using Stardust.Interstellar;
using Stardust.Interstellar.Messaging;
using Stardust.Nucleus;
using Stardust.Nucleus.Extensions;
using Stardust.Particles;

namespace Stardust.Core.Wcf
{
    public class ServiceHeaderInspector : IParameterInspector, IClientMessageInspector, IDispatchMessageInspector
    {
        public ServiceHeaderInspector()
        {

        }

        private IRuntime runtime
        {
            get
            {
                return ContainerFactory.Current.Resolve<IRuntime>(Scope.Context);
            }
            set
            {
                ContainerFactory.Current.Bind(typeof(IRuntime), value, Scope.Context);
            }
        }

        private ResponseHeader header
        {
            get
            {
                return ContainerFactory.Current.Resolve<ResponseHeader>(Scope.Context);
            }
            set
            {
                ContainerFactory.Current.Bind(typeof(ResponseHeader), value, Scope.Context);
            }
        }

        public object BeforeCall(string operationName, object[] inputs)
        {
            return null;
        }

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            try
            {
                Clean(outputs, returnValue);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        private void Clean(object[] outputs, object returnValue)
        {
            if (returnValue is ResponseHeader || returnValue is IResponseBase)
            {
                return;
            }
            if (returnValue.IsNull()) return;
            var hasHeader = outputs.Any(output => output is ResponseHeader || output is IResponseBase);
            if (hasHeader) return;
            if (runtime.IsNull()) return;
            var request = runtime.GetServiceRequest() as IRequestBase;
            header = new ResponseHeader
            {
                CallStack = runtime.CallStack,
                ExecutionTime = GetExecutionTime(),
                MessageId = Guid.NewGuid().ToString(),
                RuntimeInstance = runtime.InstanceId.ToString(),
                TimeStamp = DateTime.UtcNow,
                ServerIdentity = Environment.MachineName
            };
            if (request != null && request.RequestHeader != null)
            {
                header.ReferingMessageId = request.RequestHeader.MessageId;
                header.OriginalRuntimeInstance = request.RequestHeader.RuntimeInstance;
                header.SupportCode = request.RequestHeader.SupportCode;
            }

        }

        private long GetExecutionTime()
        {
            if (runtime.CallStack == null) return -1;
            return (long)(DateTime.UtcNow - runtime.CallStack.TimeStamp).TotalMilliseconds;
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {

            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {

        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            IStardustContext ctx = null;
            try
            {
                ctx = ContextScopeExtensions.CreateScope();
                if (request.Version.Envelope == EnvelopeVersion.None)
                {

                    runtime = RuntimeFactory.CreateRuntime();
                    return ctx;
                }
                if (request.Headers.Any(messageHeader => messageHeader.Name == "HeaderInfoIncluded"))
                {
                    runtime = RuntimeFactory.CreateRuntime();
                    return ctx;
                }
                ;
                var header = request.Headers.GetHeader<RequestHeader>("RequestHeader", RequestHeader.NS);
                request.Properties.Add("autoHeader", header);
                runtime = RuntimeFactory.CreateRuntime();
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            return ctx;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var ctx = correlationState as IStardustContext;
            try
            {
                if (reply.Version.Envelope != EnvelopeVersion.None)
                {
                    try
                    {
                        if (header.IsInstance())
                        {
                            if (reply != null && !reply.IsEmpty)
                                reply.Headers.Add(MessageHeader.CreateHeader("ResponseHeader", ResponseHeader.NS, header));
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Log();
                    }
                }
                Task.Run(() => ctx.Dispose());
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
    }
}