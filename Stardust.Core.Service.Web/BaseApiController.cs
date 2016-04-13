using System;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using System.Web.SessionState;
using Stardust.Interstellar;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Trace;
using Stardust.Nucleus;
using Stardust.Particles;
using Utilities = Stardust.Interstellar.Utilities.Utilities;

namespace Stardust.Core.Service.Web
{
    /// <summary>
    /// Defines properties and methods for API controller with Stardust initialized.
    /// </summary>  
    [SessionState(SessionStateBehavior.Disabled)]
    public abstract class BaseApiController : ApiController, IStardustController
    {
        internal const string PerfHeaderName = "X-Perf";

        internal const string SupportCodeHeaderName = "x-supportCode";

        private const string VersionHeaderName = "X-Version";

        public override async Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            var result = await base.ExecuteAsync(controllerContext, cancellationToken);
            TearDown(result);
            AddSupportCode(result);
            AppendHeaders(result);
            return result;
        }

        protected BaseApiController(IRuntime runtime)
        {
            Runtime = runtime;
            Runtime.SetEnvironment(Utilities.GetEnvironment());
            versionHandler = Resolver.Activate<IVersionResolver>();
        }
        /// <summary>
        /// the <see cref="IRuntime"/> instance for this WebApi request.
        /// </summary>
        public IRuntime Runtime { get; private set; }

        public virtual bool DoInitializationOnActionInvocation
        {
            get { return Runtime != null; }
        }

        private ITracer Tracer;

        private IVersionResolver versionHandler;

        /// <summary>
        /// Takes the CallStack note from the WCF response and adds it to the current <see cref="IRuntime"/> CallStack 
        /// </summary>
        /// <param name="result"></param>
        protected void AppendCallStackFromMessage(IResponseBase result)
        {
            if (result.ResponseHeader.CallStack.IsInstance())
                Runtime.CallStack.CallStack.Add(result.ResponseHeader.CallStack);
        }

        /// <summary>
        /// Gets the <see cref="ClaimsIdentity"/> for the request
        /// </summary>
        protected ClaimsIdentity Identity
        {
            get { return Runtime.GetCurrentClaimsIdentity(); }
        }

        /// <summary>
        /// Extends the default <see cref="Initialize"/> method on the
        /// <see cref="ApiController" /> with <see cref="IRuntime"/>. Extend this to add additional initialization steps. Remember to call the base first.
        /// initialization.
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            if (!DoInitializationOnActionInvocation)
                InitializeRuntime(controllerContext, controllerContext.RequestContext.Principal);
        }

        private void InitializeRuntime(HttpControllerContext controllerContext, IPrincipal principal)
        {
            CreateInitalizer().Initialize(controllerContext.Request.RequestUri, controllerContext.Request.Method.Method, this);
        }

        private static ControllerActivatorBase CreateInitalizer()
        {
            return new ControllerActivatorBase();
        }

        string IStardustController.GetServiceName(Uri requestUri)
        {
            return String.Format("{0}://{1}", requestUri.Scheme,
                requestUri.Host);
        }

        void IStardustController.SetTracer(ITracer tracer)
        {
            Tracer = tracer;
        }

        string IStardustController.GetMethodName(Uri requestUri, string action)
        {
            return string.Format("{0} |{1}|", requestUri.PathAndQuery, action);
        }

        internal HttpResponseMessage TearDown(HttpResponseMessage message)
        {
            if (Tracer.IsDisposed) return message;
            Tracer.Dispose();
            AddSupportCode(message);
            Runtime.TearDown(Request.RequestUri.AbsoluteUri);
            AppendHeaders(message);
            return message;
        }
        private void AddSupportCode(HttpResponseMessage message)
        {
            try
            {
                try
                {
                    if (!message.Headers.Contains(VersionHeaderName))
                        message.Headers.Add(VersionHeaderName, versionHandler != null ? versionHandler.GetVersionNumber() : "");
                }
                catch
                {
                    // ignored
                }
                if (message.Headers.Contains(SupportCodeHeaderName)) return;
                string supportCode;
                if (ApiTeardownActionFilter.TryGetSupportCode(Runtime, out supportCode))
                {
                    message.Headers.Add(SupportCodeHeaderName, supportCode);
                }
                else
                {
                    message.Headers.Add(SupportCodeHeaderName, Runtime.InstanceId.ToString());
                }

            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        private void AppendHeaders(HttpResponseMessage message)
        {
            if (Runtime.Context.GetEnvironmentConfiguration().GetConfigParameter("AddPerformanceInfo") != "false")
            {
                if (Runtime.GetTracer() != null && !Runtime.GetTracer().IsDisposed) Runtime.GetTracer().Dispose();
                if (Runtime.CallStack != null && Runtime.CallStack.ExecutionTime != null)
                    if (!message.Headers.Contains(PerfHeaderName))
                        message.Headers.Add(PerfHeaderName, string.Format("{0}", Runtime.CallStack.ExecutionTime));
            }

        }

        internal Exception TearDown(Exception exception)
        {
            if (Tracer.IsDisposed) return exception;
            Tracer.Dispose();
            Runtime.TearDown(exception);
            return exception;
        }

        protected BootstrapContext BootstrapContext
        {
            get
            {
                return (BootstrapContext)Identity.BootstrapContext;
            }
        }

        internal void TearDown(string message)
        {
            if (Tracer.IsDisposed) return;
            Tracer.Dispose();
            Runtime.TearDown(message);
        }

        /// <summary>
        /// Creates and initializes a service proxy that utilizes actAs WIF WCF requests
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected IServiceContainer<T> GetDelegateService<T>() where T : class
        {
            return Runtime.GetDelegateService<T>();
        }

        /// <summary>
        /// Creates and initializes a service proxy authenticates the application identity. NOT the actual user of the page
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected IServiceContainer<T> GetSecuredService<T>() where T : class
        {
            return Runtime.GetSecuredService<T>();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IVersionResolver
    {
        /// <summary>
        /// Gets the application version number to embed in http headers.
        /// </summary>
        /// <returns></returns>
        string GetVersionNumber();
    }
}