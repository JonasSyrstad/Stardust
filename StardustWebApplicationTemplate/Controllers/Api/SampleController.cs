using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Stardust.Core.Service.Web;

namespace StardustWebApplicationTemplate.Controllers.Api
{
    [Authorize]//Requires authentication
    public class SampleController : BaseApiController
    {
        public SampleController(Stardust.Interstellar.IRuntime runtime)
            : base(runtime)
        {
        }
    }
}
