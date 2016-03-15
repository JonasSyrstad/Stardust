using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Stardust.Interstellar.Utilities;
using Stardust.Starterkit.Proxy.Models;

namespace Stardust.Starterkit.Proxy.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            var req = WebRequest.Create(Utilities.GetConfigLocation()+"/signin") as HttpWebRequest;
            req.Method = "GET";
            req.Accept = "application/json";
            req.ContentType = "application/json";
            req.Headers.Add("Accept-Language", "en-us");
            req.UserAgent = "StardustProxy/1.0";
            ConfigCacheHelper.SetCredentials(req);
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            var resp = req.GetResponse();
            using (var reader = new StreamReader(resp.GetResponseStream()))
            {
                ViewBag.ConnectionStatus = reader.ReadToEnd();
            }
            return View();
        }
    }
}