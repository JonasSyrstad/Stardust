using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Threading;
using System.Web;
using Stardust.Core;
using Stardust.Core.Wcf;
using Stardust.Interstellar.DefaultImplementations;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Tasks;
using Stardust.Interstellar.Trace;
using Stardust.Nucleus.Extensions;
using Stardust.Nucleus.ObjectActivator;
using Stardust.Particles;

namespace Stardust.Interstellar.FrameworkInitializers
{
    internal sealed class StardustOperationInvoker : IOperationInvoker
    {
        readonly IOperationInvoker originalInvoker;

        public StardustOperationInvoker(IOperationInvoker invoker)
        {
            originalInvoker = invoker;
        }

        public object[] AllocateInputs()
        {
            return originalInvoker.AllocateInputs();
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            InitializeService(instance, inputs);
            try
            {
                var service = GetServiceBase(instance);
                GrabSynchronizationContext(service);
                var result = originalInvoker.Invoke(instance, inputs, out outputs);
                var res = TearDownService(instance, result);
                return res;
            }
            catch (Exception ex)
            {
                var resEx = TearDownService(instance, ex);
                throw resEx;
            }
        }

        private static object TearDownService(object instance, object result)
        {
            var service = GetServiceBase(instance);
            GrabSynchronizationContext(service);
            if (service.DoManualRuntimeInitialization) return result;
            var message = result as IResponseBase;
            if (!message.IsNull()) return service.TearDown(message);
            service.TearDown();
            return result;
        }

        private static void InitializeService(object instance, IEnumerable<object> inputs)
        {
            var service = GetServiceBase(instance);
            GrabSynchronizationContext(service);
            if (service.DoManualRuntimeInitialization) return;
            var request = GetRequest(inputs);
            if (request.IsInstance())
                service.Initialize(request);
            else
                service.Initialize(Utilities.Utilities.GetEnvironment(), service.GetType().GetServiceName());
        }

        private static void GrabSynchronizationContext(IServiceBase service)
        {
            if (service?.Runtime == null) return;
            var context = SynchronizationContext.Current as ThreadSynchronizationContext;
            ThreadSynchronizationContext syncContext;
            service.Runtime.GetStateStorageContainer().TryGetItem(ServiceHeaderInspector.Synccontext, out syncContext);
            if (syncContext == null) return;
            if (context == null || context.ContextId != syncContext.ContextId)
                SynchronizationContext.SetSynchronizationContext(syncContext);
        }

        private static IServiceBase GetServiceBase(object instance)
        {
            var service = instance as IServiceBase;
            if (service.IsNull())
            {
                service = ActivatorFactory.Activator.Activate<IServiceBase>(typeof(InternalServiceReplacement));
            }
            return service;
        }

        private static IRequestBase GetRequest(IEnumerable<object> inputs)
        {
            RequestHeader head = null;
            if (OperationContext.Current.IncomingMessageProperties.ContainsKey("autoHeader"))
                head = OperationContext.Current.IncomingMessageProperties["autoHeader"] as RequestHeader;
            if (head.IsInstance())
            {
                return new HeaderWrapper(head);
            }
            return inputs.IsNull()
                ? null
                : (from m in inputs where m.Implements(typeof(IRequestBase)) select m as IRequestBase).FirstOrDefault();
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            var service = GetServiceBase(instance);
            GrabSynchronizationContext(service);
            InitializeService(instance, inputs);
            return originalInvoker.InvokeBegin(instance, inputs, callback, state);
        }



        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            try
            {
                var asyncResult = originalInvoker.InvokeEnd(instance, out outputs, result);
                return TearDownService(instance, asyncResult);
            }
            catch (Exception ex)
            {
                throw TearDownService(instance, ex);
            }
        }

        private static Exception TearDownService(object instance, Exception exception)
        {
            var service = instance as IServiceBase;
            if (service == null)
            {

            };
            if (service.DoManualRuntimeInitialization) return exception;
            if (!exception.IsInstance()) throw ConstructGenericError();
            var data = service.Runtime.GetTracer();
            exception = service.TearDown(exception);
            return ConstructErrorMessage(exception, data, service);
        }

        private static FaultException<ErrorMessage> ConstructErrorMessage(Exception exception, ITracer data, IServiceBase service)
        {
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.useWcfWebFault", false))
                return new WebFaultException<ErrorMessage>(new ErrorMessage { Message = exception.Message, FaultLocation = data.GetCallstack().ErrorPath, TicketNumber = service.Runtime.InstanceId, Detail = ErrorDetail.GetDetails(exception) },
                        HttpStatusCode.InternalServerError);
                var error= new FaultException<ErrorMessage>(new ErrorMessage
                                                        {
                                                            Message = exception.Message,
                                                            FaultLocation = data.GetCallstack().ErrorPath,
                                                            TicketNumber = service.Runtime.InstanceId,
                                                            Detail = ErrorDetail.GetDetails(exception)
                                                        }, exception.Message);
            return error;

        }

        private static FaultException ConstructGenericError()
        {
            if(ConfigurationManagerHelper.GetValueOnKey("stardust.useWcfWebFault",false))
                return new WebFaultException(HttpStatusCode.InternalServerError);
            return new FaultException(new FaultReason("An unknown error occurred"));
        }

        public bool IsSynchronous
        {
            get { return originalInvoker.IsSynchronous; }
        }
    }

    internal class InternalServiceReplacement : DefaultServiceBase, IServiceBase
    {
        private readonly object serviceInstance;

        public InternalServiceReplacement(IRuntime runtime, object serviceInstance)
            : base(runtime)
        {
            this.serviceInstance = serviceInstance;
        }

        protected override ITracer InitializeWithCallerName(string environment, string serviceName, string methodName = null)
        {
            try
            {
                TokenManager.SetBootstrapToken(BootstrapContext);
                Runtime.SetBootstrapContext(BootstrapContext);
                Runtime.SetCurrentPrincipal(OperationContext.Current.ClaimsPrincipal != null && OperationContext.Current.ClaimsPrincipal.Identity.IsAuthenticated ? OperationContext.Current.ClaimsPrincipal : HttpContext.Current.User);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            if (Tracer.IsInstance()) return Tracer;
            var callerName = methodName.IsNullOrWhiteSpace() ? OperationContext.Current.IncomingMessageHeaders.Action.Split('/').LastOrDefault() : methodName;
            Runtime = RuntimeFactory.CreateRuntime();
            if (environment.ContainsCharacters())
                Runtime.SetEnvironment(environment);
            var tracer = Runtime.SetServiceName(serviceInstance, serviceName, callerName);
            tracer.GetCallstack().Name = GetServiceName();
            tracer.SetAdidtionalInformation(Runtime.InstanceId.ToString());
            Tracer = tracer;
            return tracer;
        }

        private string GetServiceName()
        {
            var name = from i in serviceInstance.GetType().GetInterfaces()
                       let attrib = Utilities.Utilities.GetServiceNameFromAttribute(i)
                       where attrib.IsInstance()
                       select attrib.ServiceName;
            var serviceName = name.FirstOrDefault();
            return serviceName.ContainsCharacters() ? serviceName : GetType().Name;
        }
    }



    internal class HeaderWrapper : RequestBase
    {
        public HeaderWrapper()
        {

        }

        public HeaderWrapper(RequestHeader head)
            : base(head)
        {

        }
    }

    internal class InvokeMarker
    {
    }
}
