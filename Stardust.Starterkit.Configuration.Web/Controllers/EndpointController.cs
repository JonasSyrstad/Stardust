using System;
using System.Linq;
using System.Web.Mvc;
using Stardust.Interstellar;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Repository;
using Stardust.Starterkit.Configuration.Web.Models;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    [Authorize]
    public class EndpointController : BaseController
    {
        private IConfigSetTask reader;

        public EndpointController(IConfigSetTask reader, IRuntime runtime)
            : base(runtime)
        {
            this.reader = reader;
        }
        public ActionResult Create(string id, string command)
        {
            
            var service = reader.GetService(command);
            ViewBag.Trail = service.GetTrail();
            if (!service.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Name = service.ConfigSet.Name;
            ViewBag.System = service.ConfigSet.System;
            ViewBag.ServiceId = service.Id;
            ViewBag.AddedBindings = (from i in service.Endpoints select i.Name).ToList();
            return View((object) null);
        }

        [HttpPost]
        public ActionResult Create(string id, string command, Endpoint model)
        {
            var service = reader.GetService(command);
            if (!service.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Name = service.ConfigSet.Name;
            ViewBag.System = service.ConfigSet.System;
            ViewBag.ServiceId = service.Id; 
            var endpoint=reader.CreateEndpoint(service, model.Name);
            return RedirectToAction("Details", new {id = "edit", item = endpoint.Id});
        }

        public ActionResult Details(string id, string item)
        {
            var endpoint = reader.GetEndpoint(item);
            ViewBag.Trail = endpoint.GetTrail();
            if (!endpoint.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.ServiceId = endpoint.ServiceDescription.Id;
            ViewBag.IsCustom = endpoint.Name == "custom";
            ViewBag.Id = endpoint.Id;
            return View(endpoint);
        }
    }
}