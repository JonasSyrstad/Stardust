using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Trace;
using Stardust.Particles;

namespace Stardust.Interstellar.DefaultImplementations
{
    /// <summary>
    /// The default implementation of <see cref="IServiceBase"/>
    /// </summary>
    public abstract class DefaultServiceBase : IServiceBase
    {
        private bool IsEnded;

        [Obsolete("should pass inn IRuntime",true)]
        protected DefaultServiceBase()
        {
            Runtime = RuntimeFactory.CreateRuntime();
        }

        /// <summary>
        /// The shared <see cref="IRuntime"/> instance for the service request
        /// </summary>
        public IRuntime Runtime { get; internal set; }

        internal ITracer Tracer;

        protected DefaultServiceBase(IRuntime runtime)
        {
            Runtime = runtime;

        }

        protected BootstrapContext BootstrapContext
        {
            get
            {
                if (OperationContext.Current.ClaimsPrincipal.IsNull()) return null;
                if (OperationContext.Current.ClaimsPrincipal.Identity.IsNull()) return null;
                return (BootstrapContext)((ClaimsIdentity)OperationContext.Current.ClaimsPrincipal.Identity).BootstrapContext;
            }
        }

        protected ClaimsIdentity Identity
        {
            get { return (ClaimsIdentity) OperationContext.Current.ClaimsPrincipal.Identity; }
        }

        public ITracer Initialize(string methodName = null)
        {
            var environment = Utilities.Utilities.GetEnvironment();
            var serviceName = GetServiceName();
            return InitializeWithCallerName(environment, serviceName, methodName);
        }

        void IServiceBase.Initialize(IRequestBase message, string methodName = null)
        {
            if(message.RequestHeader.IsNull())message.RequestHeader=new RequestHeader();
            var environment = Utilities.Utilities.GetEnvironment();
            var serviceName = Utilities.Utilities.GetServiceName();
            if (methodName.IsNullOrWhiteSpace() && message.IsInstance())
            {
                if (message.RequestHeader.MethodName.ContainsCharacters())
                    methodName = message.RequestHeader.MethodName;
            }
            InitializeWithCallerName(environment, serviceName, methodName);
            if (message.RequestHeader != null && message.RequestHeader.SupportCode.ContainsCharacters())
            {
                Runtime.TrySetSupportCode(message.RequestHeader.SupportCode);
            }
            Runtime.InitializeWithMessageContext(message);

        }

        public void Initialize(string environment, string serviceName, string methodName = null)
        {
            InitializeWithCallerName(environment, serviceName, methodName);
        }

        public virtual void TearDown()
        {
            if (IsEnded) return;
            IsEnded = true;
            if (!Tracer.IsDisposed)
            {
                Tracer.Dispose();
                Runtime.TearDown("");
            }

        }

        T IServiceBase.TearDown<T>(T message)
        {
            if (IsEnded) return message;
            IsEnded = true;
            if (Tracer.IsDisposed) return message;
            Tracer.Dispose();
            var messagereturnVal = Runtime.TearDown(message);
            return messagereturnVal;
        }

        public virtual Exception TearDown(Exception exception)
        {
            if (IsEnded) return exception;
            IsEnded = true;
            if (Tracer.IsDisposed) return exception;
            Tracer.Dispose();
            return Runtime.TearDown(exception);
        }

        private string GetServiceName()
        {
            var name = from i in GetType().GetInterfaces()
                let attrib = Utilities.Utilities.GetServiceNameFromAttribute(i)
                where attrib.IsInstance()
                select attrib.ServiceName;
            var serviceName = name.FirstOrDefault();
            return serviceName.ContainsCharacters() ? serviceName : GetType().Name;
        }

        protected virtual ITracer InitializeWithCallerName(string environment, string serviceName, string methodName = null)
        {
            try
            {
                TokenManager.SetBootstrapToken(BootstrapContext);
                Runtime.SetBootstrapContext(BootstrapContext);
                Runtime.SetCurrentPrincipal(OperationContext.Current.ClaimsPrincipal!=null&& OperationContext.Current.ClaimsPrincipal.Identity.IsAuthenticated ? OperationContext.Current.ClaimsPrincipal: HttpContext.Current.User);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            if (Tracer.IsInstance()) return Tracer;
            var callerName = GetCallerName(methodName);
            Runtime = RuntimeFactory.CreateRuntime();
            if (environment.ContainsCharacters())
                Runtime.SetEnvironment(environment);
            var tracer = Runtime.SetServiceName(this, serviceName, callerName);
            tracer.GetCallstack().Name = GetServiceName();
            tracer.SetAdidtionalInformation(Runtime.InstanceId.ToString());
            Tracer = tracer;
            return tracer;
        }

        private static string GetCallerName(string methodName)
        {
            if (OperationContext.Current == null || OperationContext.Current.IncomingMessageHeaders == null || OperationContext.Current.IncomingMessageHeaders.Action==null)
            {
                return methodName.IsNullOrWhiteSpace()
                           ? WebOperationContext.Current.IncomingRequest.UriTemplateMatch.Template.ToString()
                           : methodName;
            }
            return methodName.IsNullOrWhiteSpace() ? OperationContext.Current.IncomingMessageHeaders.Action.Split('/').LastOrDefault() : methodName;
        }

        public virtual bool DoManualRuntimeInitialization
        {
            get { return false; }
        }

    }

    /// <summary>
    /// This is a service base implementation that allows background processing on a separate thread.
    /// </summary>
    /// <remarks>Remember to do manual service initialization. when running in this mode the framework does not automatically initialize the IRuntime instance</remarks>
    public abstract class DefaultAsyncServiceBase : DefaultServiceBase
    {
        protected DefaultAsyncServiceBase(IRuntime runtime)
            : base(runtime)
        {
        }

        public override bool DoManualRuntimeInitialization
        {
            get { return true; }
        }

        protected void Initialize(IRequestBase request, string methodName = null)
        {
            ((IServiceBase)this).Initialize(request, methodName);
        }

        /// <summary>
        /// Spins up a new thread that does the actual processing allowing the service to return to the client.
        /// You should use OneWay (fire and forget) operations if you don't return any data the the client.
        /// </summary>
        /// <param name="execute">The code to run async</param>
        protected void ExecuteAsync(Action execute)
        {
            using (TracerFactory.StartTracer(this, "EnteringAsyncMode"))
            {
                var locker = Runtime.GetTracer().ExtendLifeTime();
                var context = OperationContext.Current;
                Task.Run(() => StartThread(execute, context, locker));
                Thread.Yield();
            }
        }

        private void StartThread(Action execute, OperationContext current, IDisposable locker)
        {
            OperationContext.Current = current;
            using (locker) //Causes the OperationContext to stay open and all resources available in the Context scope
            {
                try
                {
                    execute();
                    Runtime.GetTracer().WrapTracerResult();
                    TearDown();
                }
                catch (Exception exception)
                {
                    Runtime.GetTracer().WrapTracerResult();
                    TearDown(exception);
                }
            }
        }
    }
}