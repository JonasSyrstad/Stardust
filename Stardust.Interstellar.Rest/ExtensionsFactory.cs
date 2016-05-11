using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Stardust.Interstellar.Rest
{
    public static class ExtensionsFactory
    {
        private static IServiceLocator locator;

        public static void SetServiceLocator(IServiceLocator serviceLocator)
        {
            locator = serviceLocator;
        }

        internal static IServiceLocator GetLocator()
        {
            return locator;
        }

        internal static T GetService<T>()
        {
            return locator != null ? locator.GetService<T>() : default(T);
        }

        public static IEnumerable<T> GetServices<T>()
        {
            return locator?.GetServices<T>();
        }

        internal static string GetServiceTemplate(MethodInfo methodInfo)
        {
            var template = GetService<IRouteTemplateResolver>()?.GetTemplate(methodInfo);
            if (!string.IsNullOrWhiteSpace(template)) return template;
            var templateAttrib = methodInfo.GetCustomAttribute<RouteAttribute>();
            template = templateAttrib.Template;
            return template;
        }

        internal static string GetRouteTemplate(IRoutePrefixAttribute templatePrefix, RouteAttribute template, MethodInfo methodInfo)
        {
            var templateResolver = 
                GetService<IRouteTemplateResolver>();
            var route = templateResolver?.GetTemplate(methodInfo);
            if (!String.IsNullOrWhiteSpace(route)) return route;
            return templatePrefix == null ? "" : (templatePrefix.Prefix + "/") + template.Template;
        }

        internal static void BuildParameterInfo(MethodInfo methodInfo, ActionWrapper action)
        {
            var parameterHandler = GetService<IServiceParameterResolver>();
            if (parameterHandler != null)
            {
                action.Parameters.AddRange(parameterHandler.ResolveParameters(methodInfo));
            }
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

        internal static List<IHeaderHandler> GetHeaderInspectors(MethodInfo methodInfo)
        {
            var inspectors = methodInfo.GetCustomAttributes().OfType<IHeaderInspector>().ToList();
            var headerInspectors = ExtensionsFactory.GetServices<IHeaderHandler>();
            var handlers = new List<IHeaderHandler>();
            if (headerInspectors != null) handlers.AddRange(headerInspectors);
            foreach (var inspector in inspectors)
            {
                handlers.AddRange(inspector.GetHandlers());
            }
            return handlers;
        }

        internal static List<HttpMethod> GetHttpMethods(List<IActionHttpMethodProvider> actions, MethodInfo method)
        {
            var methodResolver = ExtensionsFactory.GetService<IWebMethodConverter>();
            var methods = new List<HttpMethod>();
            if (methodResolver != null) methods.AddRange(methodResolver.GetHttpMethods(method));
            foreach (var actionHttpMethodProvider in actions)
            {
                methods.AddRange(actionHttpMethodProvider.HttpMethods);
            }
            if (methods.Count == 0) methods.Add(HttpMethod.Get);
            return methods;
        }
    }
}