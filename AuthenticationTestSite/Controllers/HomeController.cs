using System.Web.Mvc;
using Stardust.Core.Service.Web;
using Stardust.Interstellar;
using Stardust.Nucleus;

namespace AuthenticationTestSite.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly IAppPoolRecycler recyler;

        public ActionResult Index()
        {
            using (var container = GetDelegateService<ITestRest>())
            {
                var result = container.GetClient().DoWork("test");
                return View(result);
                //Response.Write(result.Message);
            }
        }

        public ActionResult About()
        {
            recyler.TryRecycleCurrent();
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public HomeController(IRuntime runtime,IAppPoolRecycler recyler)
            : base(runtime)
        {
            this.recyler = recyler;
        }
    }
}