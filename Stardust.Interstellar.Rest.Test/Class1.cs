using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Stardust.Interstellar.Rest.ServiceWrapper;

namespace Stardust.Interstellar.Rest.Test
{
        [IRoutePrefix("api")]
        public interface ITestApi
        {
            [Route("test/{id}")]
            [HttpGet]
            string Apply1([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name);

            [Route("test2/{id}")]
            [HttpGet]
            string Apply2([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Header)]string item3);

            [Route("test3/{id}")]
            [HttpGet]
            string Apply3([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Header)]string item3, [In(InclutionTypes.Header)]string item4);

            [Route("put1/{id}")]
            [HttpPut]
            void Put([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Body)] DateTime timestamp);

            [Route("test5/{id}")]
            [HttpGet]
            Task<string> ApplyAsync([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Path)]string item3, [In(InclutionTypes.Path)]string item4);

            [Route("put2/{id}")]
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

    [RoutePrefix("api")]
    public class TestApi : ServiceWrapperBase<ITestApi>
    {
        public TestApi(ITestApi implementation)
            : base(implementation)
        {
            
        }

        [Route("test1/{id}",Name = "Apply1", Order = 1)]
        [HttpGet]
        public HttpResponseMessage Apply1([FromUri] string id, [FromUri] string name)
        {
            try
            {
                var parameters = new object[] { id, name };
                var serviceParameters = GatherParameters("Apply1", parameters);
                var result = base.implementation.Apply1((string)serviceParameters[0].value, (string)serviceParameters[1].value);
                return base.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(ex);
            }
        }

        [Route("test1/{id}")]
        [HttpGet]
        public  Task<HttpResponseMessage> Apply3([FromUri] string id, [FromUri] string name, [FromUri]string item3, [FromUri]string item4)
        {
            try
            {
                var parameters = new object[] { id, name,item3,item4 };
                var serviceParameters = GatherParameters("Apply3", parameters);
                var result = base.implementation.ApplyAsync((string)serviceParameters[0].value, (string)serviceParameters[1].value, (string)serviceParameters[2].value, (string)serviceParameters[3].value);
                return base.CreateResponseAsync(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Task.FromResult(CreateErrorResponse(ex));
            }
        }

        [Route("test/{id}")]
        [HttpPut]
        public Task<HttpResponseMessage> PutAsync([FromUri] string id, [FromBody] DateTime timestamp)
        {
            try
            {
                var parameters = new object[] { id, timestamp };
                var serviceParameters = GatherParameters("PutAsync", parameters);
                var result = base.implementation.PutAsync((string)serviceParameters[0].value, (DateTime)serviceParameters[1].value);
                return base.CreateResponseVoidAsync(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Task.FromResult(CreateErrorResponse(ex));
            }
        }

        [Route("test/{id}")]
        [HttpPut]
        public HttpResponseMessage Put([FromUri] string id, [FromBody] DateTime timestamp)
        {
            try
            {
                var parameters = new object[] { id, timestamp };
                var serviceParameters = GatherParameters("Put", parameters);
               implementation.Put((string)serviceParameters[0].value, (DateTime)serviceParameters[1].value);
                return CreateResponse<object>(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(ex);
            }
        }
    }
}
