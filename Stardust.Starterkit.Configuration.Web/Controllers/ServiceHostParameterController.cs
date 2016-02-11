using System;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Business;
using System.Web.Mvc;
using Stardust.Starterkit.Configuration.Repository;
using Stardust.Starterkit.Configuration.Web.Models;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    [Authorize]
    public class ServiceHostParameterController : BaseController
    {
        public ActionResult Create(string id)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var serviceHost = reader.GetServiceHost(id);
            ViewBag.Trail = serviceHost.GetTrail();
            if (!serviceHost.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.HostId = serviceHost.Id;
            return View((object)null);
        }

        [HttpPost]
        public ActionResult Create(string id, ServiceHostParameter model)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var serviceHost = reader.GetServiceHost(id);
            if (!serviceHost.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.HostId = serviceHost.Id;
            var param = reader.CreateServiceHostParameter(serviceHost, model.Name, model.IsSecureString, model.ItemValue,model.IsEnvironmental);
            return RedirectToAction("Details","ServiceHosts", new { id = serviceHost.Id });
        }

        public ActionResult Details(string id)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var param = reader.GetHostParameter(id);
            ViewBag.Trail = param.GetTrail();
            if (!param.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.HostId = param.ServiceHost.Id;
            return View(param);
        }

        [HttpPost]
        public ActionResult Details(string id, ServiceHostParameter model)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var par = reader.GetHostParameter(id);
            if (!par.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.HostId = par.ServiceHost.Id;
            par.IsEnvironmental = model.IsEnvironmental;
            if (model.ItemValue != par.ItemValue)
            {
                if (model.ItemValue.IsNullOrWhiteSpace())
                    par.ItemValue = null;
                else
                    par.SetValue(model.ItemValue);
            }
            reader.UpdateHostParameter(par);

            return RedirectToAction("Details", "ServiceHosts", new { id = par.ServiceHost.Id });
        }
    }
}