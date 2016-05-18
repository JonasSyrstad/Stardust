using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace Stardust.Interstellar.Rest.Legacy
{
    public static class WcfServiceProvider
    {
        public static void RegisterWcfAdapters()
        {
            ExtensionsFactory.SetServiceLocator(new WcfRouteResolver());
        }
    }
    class WcfRouteResolver : IRouteTemplateResolver, IWebMethodConverter, IServiceLocator, IServiceParameterResolver
    {

        public string GetTemplate(MethodInfo methodInfo)
        {
            var getAttrib = methodInfo.GetCustomAttribute<WebGetAttribute>();
            if (getAttrib != null) return getAttrib.UriTemplate;
            var invokeAttrib = methodInfo.GetCustomAttribute<WebInvokeAttribute>();
            if (invokeAttrib != null)
                return invokeAttrib.UriTemplate;
            return null;
        }

        public List<HttpMethod> GetHttpMethods(MethodInfo methodInfo)
        {
            var methods = new List<HttpMethod>();
            var getAttrib = methodInfo.GetCustomAttribute<WebGetAttribute>();
            if (getAttrib != null) methods.Add(HttpMethod.Get);
            var invokeAttrib = methodInfo.GetCustomAttribute<WebInvokeAttribute>();
            if (invokeAttrib != null)
            {
                methods.Add(new HttpMethod(invokeAttrib.Method));
            }
            return methods;
        }

        public T GetService<T>()
        {
            if (typeof(T).IsAssignableFrom(GetType())) return (T)GetService();
            return default(T);
        }

        private static object GetService()
        {
            return new WcfRouteResolver();
        }

        public IEnumerable<T> GetServices<T>()
        {
            return null;
        }

        public IEnumerable<ParameterWrapper> ResolveParameters(MethodInfo methodInfo)
        {
            if (GetHttpMethods(methodInfo).First() == HttpMethod.Get) methodInfo.GetParameters().Select(p => new ParameterWrapper { Type = p.ParameterType, In = InclutionTypes.Path, Name = p.Name }).ToArray() ;
            var others = methodInfo.GetParameters().Select(p => new ParameterWrapper { Type = p.ParameterType, In = InclutionTypes.Path, Name = p.Name }).ToArray();
            others.Last().In=InclutionTypes.Body;
            return others;
        }
    }

}
