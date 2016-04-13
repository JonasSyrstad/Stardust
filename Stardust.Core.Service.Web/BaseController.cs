using System;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Stardust.Interstellar;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Trace;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;

namespace Stardust.Core.Service.Web
{
    /// <summary>
    /// Provides methods that respond to HTTP requests that are made to an
    /// ASP.NET MVC Web site with Stardust initialized.
    /// </summary>
    [SessionState(SessionStateBehavior.Disabled)]
    public class BaseController : Controller, IStardustController
    {
        private ITracer Tracer;

        private const string VersionHeaderName = "X-Version";
        private IVersionResolver versionHandler;

        protected BaseController(IRuntime runtime)
        {
            Runtime = runtime;
            Runtime.SetEnvironment(Utilities.GetEnvironment());
            versionHandler = Nucleus.Resolver.Activate<IVersionResolver>();
        }

        public virtual bool DoInitializationOnActionInvocation
        {
            get { return Runtime != null; }
        }

        /// <summary>
        /// the <see cref="IRuntime"/> instance for this request.
        /// </summary>
        public IRuntime Runtime { get; private set; }

        /// <summary>
        /// This contains the incoming security token for the user. It is used when calling back end services. 
        /// </summary>
        protected BootstrapContext BootstrapContext
        {
            get { return Runtime.GetBootstrapContext(); }
        }

        /// <summary>
        /// Gets the <see cref="ClaimsIdentity"/> for the request
        /// </summary>
        protected ClaimsIdentity Identity
        {
            get { return (ClaimsIdentity)HttpContext.User.Identity; }
        }

        private void InitializeRuntime(RequestContext controllerContext)
        {
            CreateInitializer().Initialize(controllerContext.HttpContext.Request.Url, controllerContext.HttpContext.Request.HttpMethod, this);
        }

        string IStardustController.GetMethodName(Uri requestUri, string action)
        {
            return GetMethodName(requestUri, action);
        }

        string IStardustController.GetServiceName(Uri requestUri)
        {
            return Utilities.GetServiceName();
        }

        void IStardustController.SetTracer(ITracer tracer)
        {
            Tracer = tracer;
        }

        internal void TearDown()
        {
            if (Tracer.IsDisposed) return;
            Tracer.Dispose();
            AddSupportCode();
            Runtime.TearDown("");
            if (Runtime.Context.GetEnvironmentConfiguration().GetConfigParameter("AddPerformanceInfo") != "false")
                Response.Headers.Add(BaseApiController.PerfHeaderName, Runtime.CallStack.ExecutionTime.ToString());
        }

        internal ActionResult TearDown(ActionResult result)
        {
            
            TearDown();
            return result;
        }

        private void AddSupportCode()
        {
            try
            {

                try
                {
                    HttpContext.Response.Headers.Add(VersionHeaderName, versionHandler != null ? versionHandler.GetVersionNumber() : "");
                }
                catch
                {
                    // ignored
                }
                string supportCode;
                if (ApiTeardownActionFilter.TryGetSupportCode(Runtime, out supportCode))
                {

                    HttpContext.Response.Headers.Add(BaseApiController.SupportCodeHeaderName, supportCode);
                }
                else
                {
                    HttpContext.Response.Headers.Add(BaseApiController.SupportCodeHeaderName, Runtime.InstanceId.ToString());
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        internal Exception TearDown(Exception exception)
        {
            if (Tracer.IsDisposed) return exception;
            Tracer.Dispose();
            AddSupportCode();
            exception = Runtime.TearDown(exception);
            if (Runtime.Context.GetEnvironmentConfiguration().GetConfigParameter("AddPerformanceInfo") != "false")
                Response.Headers.Add(BaseApiController.PerfHeaderName, Runtime.CallStack.ExecutionTime.ToString());
            return exception;
        }

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

        /// <summary>
        /// Initializes data that might not be available when the constructor is
        /// called and initializes the runtime with the appropriate settings
        /// based on the <paramref name="requestContext" />
        /// </summary>
        /// <param name="requestContext">
        /// The HTTP context and route data.
        /// </param>
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (!DoInitializationOnActionInvocation)
                InitializeRuntime(requestContext);
        }
        private static ControllerActivatorBase CreateInitializer()
        {
            return new ControllerActivatorBase();
        }
        private static string GetMethodName(Uri uri, string action)
        {
            return string.Format("{0} |{1}|", uri.AbsolutePath, action);
        }
    }
}
