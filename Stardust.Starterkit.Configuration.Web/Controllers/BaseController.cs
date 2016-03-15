using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Stardust.Interstellar;
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
            CurrentUser = ConfigReaderFactory.CurrentUser;
        }

        public IConfigUser CurrentUser { get; private  set; }
    }
}