using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Stardust.Interstellar.Rest.Test;

namespace Stardust.Interstellar.Test.Controllers
{
    
    
    public class TestController : ApiController, ITestApi
    {
        public string Apply1(string id, string name)
        {
            return name;
        }

        public string Apply2(string id, string name, string item3)
        {
            return name;
        }

        public string Apply3(string id, string name, string item3, string item4)
        {
            return name;
        }

        [HttpPut]
        public void Put(string id, [FromBody]DateTime timestamp)
        {
            
        }
       
        [HttpGet]
        public Task<string> ApplyAsync(string id, string name, string item3, string item4)
        {
            return Task.FromResult(name);
        }

        public Task PutAsync(string id, DateTime timestamp)
        {
            return Task.FromResult(1);
        }

        /// <summary>Initializes the <see cref="T:System.Web.Http.ApiController" /> instance with the specified controllerContext.</summary>
        /// <param name="controllerContext">The <see cref="T:System.Web.Http.Controllers.HttpControllerContext" /> object that is used for the initialization.</param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
        }
    }
}
