using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Stardust.Particles;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    public class HelpController : Controller
    {
        
        public ActionResult Index(string id)
        {
            ViewBag.HideTrail = true;
            if (id.IsNullOrWhiteSpace())
                id = "readme";
            ViewBag.Content = System.IO.File.Exists(GetHelpFilePath(id)) ? System.IO.File.ReadAllText(GetHelpFilePath(id)) : "## File does not exist";
            return View();
        }

        private static string GetHelpFilePath(string id)
        {
            return AppDomain.CurrentDomain.BaseDirectory.Replace("bin", "") + "helpFiles\\" + id +
                   ".md";
        }
    }
}