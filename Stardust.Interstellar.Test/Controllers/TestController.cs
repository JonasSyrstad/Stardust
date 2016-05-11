//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using System.Web.Http;
//using System.Web.Http.Controllers;
//using Stardust.Interstellar.Rest.Test;

//namespace Stardust.Interstellar.Test.Controllers
//{
    
    
//    public class TestController : BaseApiController
//    {
//        private readonly ITestApi contractImpementation;

//        public TestController(ITestApi contractImpementation)
//        {
//            this.contractImpementation = contractImpementation;
//        }

//        public string Apply1(string id, string name)
//        {
//            return name;
//        }

//        public string Apply2(string id, string name, string item3)
//        {
//            return name;
//        }

//        public HttpResponseMessage Apply3(string id, string name, string item3)
//        {
//            try
//            {
//                var id_ = id;
//                var name_ = name;
//                var item3_ = ResolveFromHeader<string>(Request, "item3");
//                var item4_ = ResolveFromHeader<string>(Request, "item4");
//                return CreateResponse(HttpStatusCode.OK, contractImpementation.Apply3(id_, name_, item3_, item4_));
//            }
//            catch (UnauthorizedAccessException ex)
//            {
//                return CreateErrorResponse(HttpStatusCode.Unauthorized, ex.Message);
//            }
//            catch (Exception ex)
//            {
//                return CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
//            }
//        }

//        private T ResolveFromHeader<T>(HttpRequestMessage request, string name)
//        {
//            if (!request.Headers.Contains(name)) return default(T);
//            var content = request.Headers.GetValues(name);
//            if (content.Count() <= 1) return (T)Convert.ChangeType( content.SingleOrDefault(),typeof(T));
//            return default(T);
//        }

//        [HttpPut]
//        public void Put(string id, [FromBody]DateTime timestamp)
//        {
            
//        }
       
//        [HttpGet]
//        public Task<string> ApplyAsync(string id, string name, string item3, string item4)
//        {
//            return Task.FromResult(name);
//        }

//        public Task PutAsync(string id, DateTime timestamp)
//        {
//            return Task.FromResult(1);
//        }

//        /// <summary>Initializes the <see cref="T:System.Web.Http.ApiController" /> instance with the specified controllerContext.</summary>
//        /// <param name="controllerContext">The <see cref="T:System.Web.Http.Controllers.HttpControllerContext" /> object that is used for the initialization.</param>
//        protected override void Initialize(HttpControllerContext controllerContext)
//        {
//            base.Initialize(controllerContext);
//        }
//    }

//    public class BaseApiController : ApiController
//    {

//        private T ResolveFromHeader<T>(HttpRequestMessage request, string name)
//        {
//            if (!request.Headers.Contains(name)) return default(T);
//            var content = request.Headers.GetValues(name);
//            if (content.Count() <= 1) return (T)Convert.ChangeType(content.SingleOrDefault(), typeof(T));
//            return default(T);
//        }

//        public HttpResponseMessage CreateResponse<T>(HttpStatusCode statusCode, T message)
//        {
//            return Request.CreateResponse(statusCode, message);
//        }

//        public HttpResponseMessage CreateErrorResponse(HttpStatusCode statusCode, string message)
//        {
//            return Request.CreateErrorResponse(statusCode, message);
//        }
//    }
//}
