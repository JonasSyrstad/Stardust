#region TERS source
// Last updated by: Syrstad, Jonas
// Updated: 2016-03-08
// Stardust.Core/Stardust.Core.Service.Web/SapiController.cs
// Created:  20160308
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Particles.Collection.Arrays;

namespace Stardust.Core.Service.Web
{

    public static class ServiceImplementationRegister
    {
        public static void Register<T>(string routeName)
        {
            Resolver.GetConfigurator().Bind<IRestService>().To<RestService<T>>(routeName);
        }
    }
    public class SapiController : BaseApiController
    {
        public SapiController(IRuntime runtime)
            : base(runtime)
        {
        }

        protected virtual bool RethrowExceptions
        {
            get
            {
                return false;
            }
        }

        [HttpGet]
        [HttpPut]
        [HttpPost]
        [HttpPatch]
        [Route("sapi/{serviceImplementation}/{method}/{id}")]
        public virtual HttpResponseMessage Dynamic(string serviceImplementation, string method, [BindCatchAllRoute('/')]object[] parameters, [FromBody]object body)
        {
            var srv = Resolver.Activate<IRestService>(serviceImplementation);
            var instance = srv.ServiceType.Activate(Scope.Context);

            var cacheName = string.Format("{0}.{1}", serviceImplementation, method);
            Func<object, object[], object> m;
            if (!setExpressionCache.TryGetValue(cacheName, out m))
            {
                var minfo = srv.ServiceType.GetMethod(method);
                m = CreateSet(minfo);
                setExpressionCache.TryAdd(cacheName, m);
            }
            try
            {
                if (parameters.ContainsElements() && body != null)
                {
                    var list = new List<object>(parameters) { body };
                    return Request.CreateResponse(m(instance, list.ToArray()));
                }
                if (parameters.ContainsElements() && body == null) return Request.CreateResponse(m(instance, parameters ));
                if (body != null)
                {
                    return Request.CreateResponse(m(instance, new[] { body }));
                }
                return Request.CreateResponse(m(instance, new object[0]));
            }
            catch (Exception ex)
            {
                ex.Log();
                if (RethrowExceptions) throw;
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        private static readonly ConcurrentDictionary<string, Func<object, object[], object>> setExpressionCache = new ConcurrentDictionary<string, Func<object, object[], object>>();
        private static Func<object, object[], object> CreateSet(MethodInfo field)
        {
            try
            {
                var param = field.ReturnType;
                var valueParameter = Expression.Parameter(typeof(object[]), "value");
                var pars = new List<Expression>();
                var i = 0;
                foreach (var p in field.GetParameters())
                {
                    pars.Add(Expression.Convert(Expression.Parameter(p.ParameterType, p.Name), p.ParameterType));
                }
                var instanceParameter = Expression.Parameter(typeof(object), "target");
                var member = Expression.Call(Expression.Convert(instanceParameter, field.DeclaringType), field.Name, null, pars.ToArray());


                var lambda = Expression.Lambda<Func<object, object[], object>>(member, instanceParameter, valueParameter);
                return lambda.Compile();
            }
            catch (Exception ex)
            {
                Logging.Exception(ex);
                throw;
            }
        }
    }

    /// <summary>
    /// this is a marker interface.
    /// </summary>
    internal interface IRestService
    {
        Type ServiceType { get; }
    }

    internal interface IRestService<T> : IRestService
    {
    }

    class RestService<T> : IRestService<T>
    {
        public Type ServiceType
        {
            get
            {
                return typeof(T);
            }
        }
    }

    public class CatchAllRouteParameterBinding : HttpParameterBinding
    {

        private readonly string _parameterName;
        private readonly char _delimiter;

        public CatchAllRouteParameterBinding(
            HttpParameterDescriptor descriptor, char delimiter)
            : base(descriptor)
        {

            _parameterName = descriptor.ParameterName;
            _delimiter = delimiter;
        }

        public override Task ExecuteBindingAsync(
            System.Web.Http.Metadata.ModelMetadataProvider metadataProvider,
            HttpActionContext actionContext,
            CancellationToken cancellationToken)
        {

            var routeValues = actionContext.ControllerContext.RouteData.Values;

            if (routeValues[_parameterName] != null)
            {

                string[] catchAllValues =
                    routeValues[_parameterName].ToString().Split(_delimiter);

                actionContext.ActionArguments.Add(_parameterName, catchAllValues);
            }
            else
            {

                actionContext.ActionArguments.Add(_parameterName, new string[0]);
            }

            return Task.FromResult(0);
        }
    }
    public class BindCatchAllRouteAttribute : ParameterBindingAttribute
    {

        private readonly char _delimiter;

        public BindCatchAllRouteAttribute(char delimiter)
        {

            _delimiter = delimiter;
        }

        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {

            return new CatchAllRouteParameterBinding(parameter, _delimiter);
        }
    }
}