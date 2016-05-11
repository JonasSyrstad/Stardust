using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Stardust.Interstellar.Rest.ServiceWrapper
{
    public abstract class ServiceWrapperBase<T> : ApiController
    {
        protected readonly T implementation;

        protected HttpResponseMessage CreateResponse<TMessage>(HttpStatusCode statusCode, TMessage message =default(TMessage))
        {
            if (message == null) return Request.CreateResponse(HttpStatusCode.NoContent);
            return Request.CreateResponse(statusCode, message);
        }

        protected async Task<HttpResponseMessage> CreateResponseAsync<TMessage>(HttpStatusCode statusCode, Task<TMessage> messageTask = null)
        {
            try
            {
                var message = await messageTask;
                if (message == null) return Request.CreateResponse(HttpStatusCode.NoContent);
                return Request.CreateResponse(statusCode, message);
            }
            catch (Exception ex)
            {

                return CreateErrorResponse(ex);
            }
        }

        protected async Task<HttpResponseMessage> CreateResponseVoidAsync(HttpStatusCode statusCode, Task messageTask)
        {
            try
            {
                await messageTask;
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {

                return CreateErrorResponse(ex);
            }
        }

        protected HttpResponseMessage CreateErrorResponse(Exception ex)
        {
            //TODO: add more exception conventions..
            if (ex is UnauthorizedAccessException) return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex.Message);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        }

        private static ConcurrentDictionary<Type, ConcurrentDictionary<string, ActionWrapper>> cache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, ActionWrapper>>();

        protected ServiceWrapperBase(T implementation)
        {
            this.implementation = implementation;
            InitializeServiceApi(typeof(T));
        }

        protected ParameterWrapper[] GatherParameters(string name, object[] fromWebMethodParameters)
        {
            try
            {
                ConcurrentDictionary<string, ActionWrapper> item;
                if (!cache.TryGetValue(typeof(T), out item)) throw new InvalidOperationException("Invalid interface type");
                ActionWrapper action;
                if (!item.TryGetValue(GetActionName(name), out action)) throw new InvalidOperationException("Invalid action");
                var i = 0;
                var wrappers = new List<ParameterWrapper>();
                foreach (var parameter in action.Parameters)
                {
                    var val = parameter.In == InclutionTypes.Header ? GetFromHeaders(parameter) : fromWebMethodParameters[i];
                    wrappers.Add(parameter.Create(val));
                    i++;
                }
                return wrappers.ToArray();
            }
            catch (Exception ex)
            {

                throw new ParameterReflectionException(string.Format("Unable to gather parameters for {0}", name),ex);
            }

        }

        private object GetFromHeaders(ParameterWrapper parameter)
        {
            if (!Request.Headers.Contains(parameter.Name)) return null;
            var vals = Request.Headers.GetValues(parameter.Name);
            if (vals.Count() <= 1) return vals.SingleOrDefault();
            throw new InvalidCastException("Not currently able to deal with collections...");
        }

        protected void InitializeServiceApi(Type interfaceType)
        {
            ConcurrentDictionary<string, ActionWrapper> wrapper;
            if (cache.TryGetValue(interfaceType, out wrapper)) return;
            var newWrapper = new ConcurrentDictionary<string, ActionWrapper>();

            foreach (var methodInfo in interfaceType.GetMethods())
            {
                
                var template = ExtensionsFactory.GetServiceTemplate(methodInfo);
                var actionName = GetActionName(methodInfo);
                var action = new ActionWrapper { Name = actionName, ReturnType = methodInfo.ReturnType, RouteTemplate = template, Parameters = new List<ParameterWrapper>() };
                var actions = methodInfo.GetCustomAttributes(true).OfType<IActionHttpMethodProvider>();
                var methods = ExtensionsFactory.GetHttpMethods(actions.ToList(),methodInfo);
                var handlers = ExtensionsFactory.GetHeaderInspectors(methodInfo);
                action.CustomHandlers = handlers;
                action.Actions = methods;
                BuildParameterInfo(methodInfo, action);
                newWrapper.TryAdd(action.Name, action);
            }
            if (cache.TryGetValue(interfaceType, out wrapper)) return;
            cache.TryAdd(interfaceType, newWrapper);
        }

        

        private static List<IHeaderHandler> GetHeaderInspectors(MethodInfo methodInfo)
        {
            var inspectors = methodInfo.GetCustomAttributes().OfType<IHeaderInspector>();
            var handlers = new List<IHeaderHandler>();
            foreach (var inspector in inspectors)
            {
                handlers.AddRange(inspector.GetHandlers());
            }
            return handlers;
        }

        private static List<HttpMethod> GetHttpMethods(IEnumerable<IActionHttpMethodProvider> actions)
        {
            var methods = new List<HttpMethod>();
            foreach (var actionHttpMethodProvider in actions)
            {
                methods.AddRange(actionHttpMethodProvider.HttpMethods);
            }
            if (methods.Count == 0) methods.Add(HttpMethod.Get);
            return methods;
        }

        protected string GetActionName(MethodInfo methodInfo)
        {
            var actionName = methodInfo.Name;
            return GetActionName(actionName);
        }

        private static string GetActionName(string actionName)
        {
            if (actionName.EndsWith("Async")) actionName = actionName.Replace("Async", "");
            return actionName;
        }

        private static void BuildParameterInfo(MethodInfo methodInfo, ActionWrapper action)
        {
            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                var @in = parameterInfo.GetCustomAttribute<InAttribute>(true);
                if (@in == null)
                {
                    var fromBody = parameterInfo.GetCustomAttribute<FromBodyAttribute>(true);
                    if (fromBody != null)
                        @in = new InAttribute(InclutionTypes.Body);
                    if (@in == null)
                    {
                        var fromUri = parameterInfo.GetCustomAttribute<FromUriAttribute>(true);
                        if (fromUri != null)
                            @in = new InAttribute(InclutionTypes.Path);
                    }
                }
                action.Parameters.Add(new ParameterWrapper { Name = parameterInfo.Name, Type = parameterInfo.ParameterType, In = @in?.InclutionType ?? InclutionTypes.Body });
            }
        }
    }

    public class ParameterReflectionException : Exception
    {
        
        public ParameterReflectionException(string message) : base(message)
        {
        }

        public ParameterReflectionException(string message, Exception innerException):base(message,innerException)
        {
           
        }

        protected ParameterReflectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
