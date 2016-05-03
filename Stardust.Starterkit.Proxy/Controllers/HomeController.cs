using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR.Client;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;
using Stardust.Starterkit.Proxy.App_Start;
using Stardust.Starterkit.Proxy.Models;

namespace Stardust.Starterkit.Proxy.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Health()
        {
            try
            {
                var req = WebRequest.Create(Utilities.GetConfigLocation() + "/Account/Signin") as HttpWebRequest;
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
                    var status= reader.ReadToEnd();
                    Logging.DebugMessage("Connection status: {0}",status);
                }

            }
            catch (Exception ex)
            {
                ex.Log();
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError,"Not connected to nexus");
            }
            if (Startup.hubConnection != null)
            {
                if(Startup.hubConnection.State==ConnectionState.Connecting||Startup.hubConnection.State==ConnectionState.Connected)
                    return new HttpStatusCodeResult(HttpStatusCode.OK, "Healthy");
            }
            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError,"Notification service not connected to nexus");
        }
        // GET: Home
        public ActionResult Index()
        {
            try
            {
                var req = WebRequest.Create(Utilities.GetConfigLocation() + "/Account/Signin") as HttpWebRequest;
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
                
            }
            catch (Exception ex)
            {
               ex.Log();

                ViewBag.ConnectionStatus = "Not connected";
            }
            if (Startup.hubConnection != null)
            {
                ViewBag.NotificationStatus = Startup.hubConnection.State.ToString();
            }
            return View();
        }
    }
}