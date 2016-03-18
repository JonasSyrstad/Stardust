using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using System.Web.Routing;
using Excel.Log;
using Stardust.Interstellar;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IRuntime Runtime;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Web.Mvc.Controller"/> class.
        /// </summary>
        protected BaseController(IRuntime runtime)
        {
            this.Runtime = runtime;
        }

        protected void ValidateAccessToSet(string setName)
        {
            if(CurrentUser.AdministratorType==AdministratorTypes.SystemAdmin)
            if(CurrentUser.ConfigSet.All(c => c.Id != setName))
                throw new UnauthorizedAccessException("Forbidden");

        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            try
            {
                CurrentUser = ConfigReaderFactory.CurrentUser;
            }
            catch (Exception ex)
            {
                ex.Log();
                Logging.DebugMessage("Current user: {0}",User.Identity.Name);
                var cp = ControllerContext.RequestContext.HttpContext.User.Identity as ClaimsIdentity;
                if(cp!=null)
                {
                    foreach (var claim in cp.Claims)
                    {
                        Logging.DebugMessage("{0} -{1}",claim.Type,claim.Value);
                    }
                }
                throw;
            }
        }

        public IConfigUser CurrentUser { get; private  set; }
    }
}