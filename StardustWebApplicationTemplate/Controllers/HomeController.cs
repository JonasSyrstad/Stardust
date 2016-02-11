using System.Web.Mvc;
using Stardust.Core.Service.Web;
using Stardust.Interstellar;

namespace StardustWebApplicationTemplate.Controllers
{
    public class HomeController : BaseController
    {
        /// <summary>
        /// Requires authentication
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Public page available to anonymous users
        /// </summary>
        /// <param name="supportId"></param>
        /// <returns></returns>
        public ActionResult Error(string supportId)
        {
            //A correlation id used in the logging system.
            ViewBag.SupportId = supportId;
            return View();
        }

        public HomeController(IRuntime runtime)
            : base(runtime)
        {
        }
    }
}