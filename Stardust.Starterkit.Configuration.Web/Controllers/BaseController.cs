using System.Web.Mvc;
using System.Web.Routing;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            CurrentUser = ConfigReaderFactory.CurrentUser;
        }

        public IConfigUser CurrentUser { get; private  set; }
    }
}