using System.Web.Security;
using Stardust.Starterkit.Configuration.Business;
using System.Web.Mvc;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    [Authorize()]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
           var providers= Roles.Providers;
            var csPresenter = ConfigReaderFactory.GetConfigSetTask();            
            return View(csPresenter.GetAllConfitSets());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}