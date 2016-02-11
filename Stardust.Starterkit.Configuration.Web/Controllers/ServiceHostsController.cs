using System;
using System.Collections.Generic;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Repository;
using System.Web.Mvc;
using Stardust.Starterkit.Configuration.Web.Models;
using System.Linq;
using Stardust.Particles;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    public class ServiceHostsController : BaseController
    {
        [Authorize]
        public ActionResult Details(string id)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var serviceHost = reader.GetServiceHost(id);
            ViewBag.Trail = serviceHost.GetTrail();
            if (!serviceHost.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Name = serviceHost.ConfigSet.Name;
            ViewBag.System = serviceHost.ConfigSet.System;
            ViewBag.HostId = serviceHost.Id;
            if (serviceHost.Visualization.IsNullOrWhiteSpace()) serviceHost.Visualization = "ellipse";
            ViewBag.Visializations = new List<string> { "ellipse", "circle", "database", "box" };
            CreateLayerSelectList(serviceHost);
            return View(serviceHost);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Details(string id, ServiceHostSettings model)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var serviceHost = reader.GetServiceHost(id);
            ViewBag.Trail = serviceHost.GetTrail();
            if (!serviceHost.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Name = serviceHost.ConfigSet.Name;
            ViewBag.System = serviceHost.ConfigSet.System;
            ViewBag.HostId = serviceHost.Id;
            CreateLayerSelectList(serviceHost);
            serviceHost.Description = model.Description;
            serviceHost.Visualization = model.Visualization;
            serviceHost.Level = model.Level;
            serviceHost.Alias = model.Alias;
            if (model.Name.ContainsCharacters() && (model.Name != serviceHost.Name))
                serviceHost.Name = model.Name;
            reader.UpdateServiceHost(serviceHost);
            ViewBag.Visializations = new List<string> { "ellipse", "circle", "database", "box" };
            return View(serviceHost);
        }

        private void CreateLayerSelectList(IServiceHostSettings serviceHost)
        {
            if (serviceHost.ConfigSet.LayerNames.ContainsCharacters())
            {
                int i = 0;
                ViewBag.Levels = (from l in serviceHost.ConfigSet.LayerNames.Split('|')
                                  let index = i = i + 1
                                  select new { Name = l, Id = index.ToString() }).ToArray();
            }
            else
            {
                ViewBag.Levels = new[]
                                     {
                                         new { Name = "1", Id = "1" }, new { Name = "2", Id = "2" }, new { Name = "3", Id = "3" },
                                         new { Name = "4", Id = "4" }, new { Name = "5", Id = "5" }, new { Name = "6", Id = "6" },
                                         new { Name = "7", Id = "7" }
                                     };
            }
        }

        public ActionResult Create(string id)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var configSet = reader.GetConfigSet(id);
            if (!configSet.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Id = configSet.Id;
            ViewBag.Name = configSet.Name;
            ViewBag.System = configSet.System;
            return View((object)null);
        }
        [HttpPost]
        public ActionResult Create(string id, ServiceHostSettings model)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var configSet = reader.GetConfigSet(id);
            if (!configSet.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Id = configSet.Id;
            var serviceHost = reader.CreateServiceHost(configSet, model.Name);
            return RedirectToAction("Details", new { id = serviceHost.Id });
        }

        public ActionResult AddDownstreamHost(string id)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var host = reader.GetServiceHost(id);
            ViewBag.Trail = host.GetTrail();
            if (!host.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Id = host.Id;
            return View(new HostsModel
                        {
                            Id = host.Id,
                            AwailableHosts = host.ConfigSet.ServiceHosts.Where(sh => sh.Id != host.Id).Where(sh => !host.DownstreamHosts.Select(h => h.Name).Contains(sh.Name)).ToList(),
                            SelectedHost = ""

                        });
        }

        [HttpPost]
        public ActionResult AddDownstreamHost(string id, HostsModel model)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var host = reader.GetServiceHost(id);
            if (!host.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Id = host.Id;
            var connection = host.ConfigSet.ServiceHosts.SingleOrDefault(s => s.Id == model.SelectedHost);
            host.DownstreamHosts.Add(connection);
            reader.UpdateServiceHost(host);
            return RedirectToAction("Details", new { id = host.Id });
        }

        public ActionResult AddUpstreamHost(string id)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var host = reader.GetServiceHost(id);
            ViewBag.Trail = host.GetTrail();
            if (!host.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Id = host.Id;
            return View(new HostsModel
            {
                Id = host.Id,
                AwailableHosts = host.ConfigSet.ServiceHosts.Where(sh => sh.Id != host.Id).Where(sh => !host.UpstreamHosts.Select(h => h.Name).Contains(sh.Name)).ToList(),
                SelectedHost = ""

            });
        }

        [HttpPost]
        public ActionResult AddUpstreamHost(string id, HostsModel model)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var host = reader.GetServiceHost(id);
            if (!host.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Id = host.Id;
            var connection = host.ConfigSet.ServiceHosts.SingleOrDefault(s => s.Id == model.SelectedHost);
            host.UpstreamHosts.Add(connection);
            reader.UpdateServiceHost(host);
            return RedirectToAction("Details", new { id = host.Id });
        }
    }
}

public class HostsModel
{
    public string Id { get; set; }

    public List<IServiceHostSettings> AwailableHosts { get; set; }

    public string SelectedHost { get; set; }
}