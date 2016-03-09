using System;
using System.Linq;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Repository;
using System.Web.Mvc;
using Stardust.Starterkit.Configuration.Web.Models;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    [Authorize]
    public class ServiceController : BaseController
    {

        private readonly IConfigSetTask reader;

        public ServiceController( IConfigSetTask configSetTasks)
        {
            this.reader = configSetTasks;
        }
        public ActionResult Create(string id, string command)
        {
            var cs = reader.GetConfigSet(command);
            if (!cs.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Trail = cs.GetTrail();
            ViewBag.Name = cs.Name;
            ViewBag.System = cs.System;
            return View();
        }

        [HttpPost]
        public ActionResult Create(string id, string command, ServiceDescription model)
        {
            var cs = reader.GetConfigSet(command);
            if (!cs.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Name = cs.Name;
            ViewBag.System = cs.System;
            reader.CreateService(cs, model.Name);
            return RedirectToAction("Details", "ConfigSet", new { name = cs.Name, system = cs.System });
        }

        public ActionResult Details(string id, string item)
        {
            var service = reader.GetService(item);
            ViewBag.Trail = service.GetTrail();
            if (service.ServiceHost.IsInstance() && service.ServiceHostId.IsNullOrEmpty()) service.ServiceHostId = service.ServiceHost.Id;
            ViewBag.ServiceHosts = service.ConfigSet.ServiceHosts;
            if (!service.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Name = service.ConfigSet.Name;
            ViewBag.System = service.ConfigSet.System;
            ViewBag.ServiceId = service.Id;
            return View(service);
        }

        [HttpPost]
        public ActionResult Details(string id, string item, ServiceDescription model)
        {
            var service = reader.GetService(item);
            ViewBag.ServiceHosts = service.ConfigSet.ServiceHosts;
            if (!service.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            service.ClientEndpointValue = model.ClientEndpointValue;
            service.ServiceHost = service.ConfigSet.ServiceHosts.SingleOrDefault(sh => sh.Id == model.ServiceHostId);
            service.ServiceHostId = model.ServiceHostId;
            service.Description = model.Description;
            if (service.ServiceHost.IsNull()) service.ServiceHostId = null;
            reader.UpdateService(service);
            return RedirectToAction("Details", "ConfigSet", new { name = service.ConfigSet.Name, system = service.ConfigSet.System });
        }

        public ActionResult Delete(string id)
        {
            var endpoint = reader.GetEndpoint(id);
            ViewBag.Trail = endpoint.GetTrail();
            return View(endpoint);
        }

        [HttpPost]
        public ActionResult Delete(string id, Endpoint model)
        {
            var endpoint = reader.GetEndpoint(id);
            reader.DeleteEndpoint(endpoint);
            return RedirectToAction("Details", new { id = "edit", item = endpoint.ServiceNameId });
        }

        public ActionResult DeleteService(string id)
        {
            var service = reader.GetService(id);
            ViewBag.Trail = service.GetTrail();
            return View(service);
        }

        [HttpPost]
        public ActionResult DeleteService(string id, ServiceDescription model)
        {
            var service = reader.GetService(id);
            var route = new { name = service.ConfigSet.Name, system = service.ConfigSet.System };
            reader.DeleteService(service);
            return RedirectToAction("Details", "ConfigSet", route);
        }
    }
}