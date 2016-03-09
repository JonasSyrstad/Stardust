using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Stardust.Rest.Client
{
    public class RestWrapper : IRestWrapper
    {
        private readonly SessionHandler session;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public RestWrapper(SessionHandler session)
        {
            this.session = session;
        }

        public Task<object> InvokeMemberPostAsync(object request, Type resultType)
        {
            try
            {
                var result = session.PostDataAsync(GetMethodNameFull(request), request, resultType);
                return result;
            }
            catch (ServiceException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ConstructNewErrorMessage(request, ex);
            }
        }

        public object InvokeMemberPost(object request, Type resultType)
        {
            try
            {
                var result = session.PostData(GetMethodNameFull(request), request, resultType);
                return result;
            }
            catch (ServiceException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ConstructNewErrorMessage(request, ex);
            }
        }

        private Exception ConstructNewErrorMessage(object request, Exception ex)
        {
            var name = GetMethodName(request);
            return new ServiceException(string.Format("Unable to invoke {0} on {1}", name, ApiName), ex);
        }

        private object GetMethodName(object request)
        {
            ConcurrentDictionary<Type, string> map;
            if (!typeMethodMap.TryGetValue(this.GetType(), out map)) return "";
            string methodName;
            if (!map.TryGetValue(request.GetType(), out methodName)) return "";
            return methodName;
        }

        //TODO: find the called method
        private string GetMethodNameFull(object request)
        {
            var methodName = GetMethodName(request);
            return string.Format("{0}/{1}/", ApiName, methodName);
        }

        private string ApiName
        {
            get
            {

                var apiName = this.GetType().GetAttribute<ApiNameAttribute>();
                if (apiName == null)
                {
                    foreach (var i in this.GetType().GetInterfaces())
                    {
                        apiName = i.GetAttribute<ApiNameAttribute>();
                        if (apiName != null)
                        {

                            break;
                        }
                    }
                }
                MapMethods(this.GetType());
                return apiName.ApiName;
            }
        }
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, string>> typeMethodMap = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, string>>();

        private void MapMethods(Type type)
        {
            ConcurrentDictionary<Type, string> typeMap;
            if (typeMethodMap.TryGetValue(type, out typeMap)) return;
            typeMap = new ConcurrentDictionary<Type, string>();
            foreach (var methodInfo in type.GetMethods())
            {
                var bindingParameter = methodInfo.GetParameters().FirstOrDefault();
                if (methodInfo.Name.Contains("Async")) continue;
                if (bindingParameter == null) continue;
                var binderType = bindingParameter.ParameterType;
                var methoodName = methodInfo.Name;
                typeMap.TryAdd(binderType, methoodName);
            }
            typeMethodMap.TryAdd(type, typeMap);
        }



        public async Task<TOut> RunTypedPostAsync<TOut>(object request)
        {
            return (TOut)await this.InvokeMemberPostAsync(request, typeof(TOut));
        }

        public Task<object> InvokeMemberGetAsync(object request, Type resultType)
        {
            throw new NotImplementedException();
        }

        public object InvokeMemberGet(object request, Type resultType)
        {
            throw new NotImplementedException();
        }

        public async Task<TOut> RunTypedGetAsync<TOut>(object request)
        {
             return (TOut)await this.InvokeMemberGetAsync(request, typeof(TOut));
        }

        public Task<object> InvokeMemberPutAsync(object request, Type resultType)
        {
            throw new NotImplementedException();
        }

        public object InvokeMemberPut(object request, Type resultType)
        {
            throw new NotImplementedException();
        }

        public Task<TOut> RunTypedPutAsync<TOut>(object request)
        {
            throw new NotImplementedException();
        }

        public Task<object> InvokeMemberDeleteAsync(object request, Type resultType)
        {
            throw new NotImplementedException();
        }

        public object InvokeMemberDelete(object request, Type resultType)
        {
            throw new NotImplementedException();
        }

        public Task<TOut> RunTypedDeleteAsync<TOut>(object request)
        {
            throw new NotImplementedException();
        }
    }
}