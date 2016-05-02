using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Stardust.Interstellar.Rest.Test
{
    public interface ITestApi
    {
        [Route("test1/{id}")]
        [HttpGet]
        string Apply1([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name);

        [Route("test2/{id}")]
        [HttpGet]
        string Apply2([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Header)]string item3);

        [Route("test/{id}")]
        [HttpGet]
        string Apply3([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Header)]string item3, [In(InclutionTypes.Header)]string item4);

        [Route("test/{id}")]
        [HttpPut]
        void Put([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Body)] DateTime timestamp);

        [Route("test/{id}")]
        [HttpGet]
        Task<string> ApplyAsync([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Path)]string item3, [In(InclutionTypes.Path)]string item4);

        [Route("test/{id}")]
        [HttpPut]
        Task PutAsync([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Body)] DateTime timestamp);
    }
    public class TestClient : RestWrapper, ITestApi
    {


        public TestClient(IAuthenticationHandler authenticationHandler, IHeaderHandlerFactory headerHandlers,TypeWrapper interfaceType)
            : base(authenticationHandler, headerHandlers, interfaceType)
        {

        }

        public string Apply1(string id, string name)
        {
            const string apply = "Apply";
            var par = new object[] { id, name };
            var parameters = GetParameters(apply, par);
            var result = Invoke<string>(apply, parameters);
            return result;
        }

        public string Apply2(string id, string name, string item3)
        {
            const string apply = "Apply";
            var par = new object[] { id, name, item3 };
            var parameters = GetParameters(apply, par);
            var result = Invoke<string>(apply, parameters);
            return result;
        }


        public string Apply3(string id, string name, string item3, string item4)
        {
            const string apply = "Apply";
            var par = new object[] { id, name, item3, item4 };
            var parameters = GetParameters(apply, par);
            var result = Invoke<string>(apply, parameters);
            return result;
        }

        public void Put(string id, DateTime timestamp)
        {
            const string apply = "Put";
            var par = new object[] { id, timestamp };
            var parameters = GetParameters(apply, par);
            InvokeVoid(apply, parameters);
        }

        public Task<string> ApplyAsync(string id, string name, string item3, string item4)
        {
            const string apply = "ApplyAsync";
            var par = new object[] { id, name, item3, item4 };
            var parameters = GetParameters(apply, par);
            var result = InvokeAsync<string>(apply, parameters);
            return result;
        }

        public Task PutAsync(string id, DateTime timestamp)
        {
            const string apply = "PutAsync";
            var par = new object[] { id, timestamp };
            var parameters = GetParameters(apply, par);
            var result = InvokeAsync<int>(apply, parameters);
            return result;
        }
    }
}
