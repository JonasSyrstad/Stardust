using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Stardust.Core.CrossCutting;
using Stardust.Core.FactoryHelpers;
using Stardust.Core.Services.ConfigurationReader;

namespace Stardust.Core.Services.ConfigurationAdministration.Controllers
{
    [Authorize]
    public class ConfigurationController : Controller
    {
        public ActionResult Index()
        {
            return View(GetConfigurationReader().GetAllSets());
        }

        public ActionResult Create()
        {
            return View(new ConfigurationSet { Created = DateTime.Now, LastUpdated = DateTime.Now });
        }

        [HttpPost]
        public ActionResult Create(ConfigurationSet model)
        {
            model.Services = new List<ServiceConfig>();
            model.Parameters = new List<ConfigParameter>();
            model.Endpoints = new List<EndpointConfig>();
            model.Environments = new List<EnvironmentConfig>();
            GetConfigurationReader().WriteConfigurationSet(model, model.SetName);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(string id)
        {
            return View(GetConfigurationReader().GetConfiguration(id));
        }

        [HttpPost]
        public ActionResult Edit(string id, ConfigurationSet model)
        {
            var item = GetConfigurationReader().GetConfiguration(id);
            item.SetName = model.SetName;
            item.ParentSet = model.ParentSet;
            item.LastUpdated = DateTime.Now;
            GetConfigurationReader().WriteConfigurationSet(item, id);
            return View(item);
        }

        public ActionResult EndpointIndex(string id)
        {
            ViewBag.Id = id;
            var item = GetConfigurationReader().GetConfiguration(id);
            return View(from e in item.Endpoints where !e.Deleted select e);
        }

        public ActionResult CreateEndpoint(string id)
        {
            ViewBag.Id = id;
            var item = GetConfigurationReader().GetConfiguration(id);
            if (item.Endpoints.IsNull()) item.Endpoints = new List<EndpointConfig>();
            return View(new EndpointConfig { Id = item.Endpoints.Count });
        }

        [HttpPost]
        public ActionResult CreateEndpoint(string id, EndpointConfig model)
        {
            ViewBag.Id = id;
            var item = GetConfigurationReader().GetConfiguration(id);
            model.Endpoints = new List<Endpoint>();
            model.Id = item.Endpoints.Count;
            item.Endpoints.Add(model);
            GetConfigurationReader().WriteConfigurationSet(item, id);
            return RedirectToAction("EndpointIndex", new { id });
        }

        public ActionResult EditEndpoint(string id, int eid)
        {
            ViewBag.Id = id;
            ViewBag.Eid = eid;
            var item = GetConfigurationReader().GetConfiguration(id);
            var endpoint = from e in item.Endpoints where e.Id == eid select e;
            return View(endpoint.First());
        }

        [HttpPost]
        public ActionResult EditEndpoint(string id, int eid, EndpointConfig model)
        {
            var item = GetConfigurationReader().GetConfiguration(id);
            ViewBag.Id = id;
            ViewBag.Eid = eid;
            var endpoint = GetEndpoint(item, id, eid);
            endpoint.ServiceName = model.ServiceName;
            endpoint.Deleted = model.Deleted;
            endpoint.ActiveEndpoint = model.ActiveEndpoint;
            GetConfigurationReader().WriteConfigurationSet(item, id);
            return RedirectToAction("EndpointIndex", new { id });
        }

        public ActionResult CreateEndpointBinding(string id, int eid)
        {
            ViewBag.Id = id;
            ViewBag.Eid = eid;
            return View(new Endpoint());
        }

        [HttpPost]
        public ActionResult CreateEndpointBinding(string id, int eid, Endpoint model)
        {
            ViewBag.Id = id;
            ViewBag.Eid = eid;
            var item = GetConfigurationReader().GetConfiguration(id);
            model.Id = item.Endpoints[eid].Endpoints.Count;
            item.Endpoints[eid].Endpoints.Add(model);
            GetConfigurationReader().WriteConfigurationSet(item, item.SetName);
            return RedirectToAction("EndpointIndex", new { id });
        }

        public ActionResult EditEndpointBinding(string id, int eid, int ebid)
        {
            var item = GetConfigurationReader().GetConfiguration(id);
            var ep = GetEndpoint(item, id, eid);
            var epb = from e in ep.Endpoints where e.Id == ebid select e;
            return View(epb.First());
        }

        [HttpPost]
        public ActionResult EditEndpointBinding(string id, int eid, int ebid, Endpoint model)
        {
            var item = GetConfigurationReader().GetConfiguration(id);
            var ep = GetEndpoint(item, id, eid);
            ep.Endpoints[ebid] = model;
            GetConfigurationReader().WriteConfigurationSet(item, id);
            return RedirectToAction("EditEndpoint", new { id, eid });
        }

        public ActionResult DeleteEndpointBinding(string id, int eid, int ebid)
        {
            var item = GetConfigurationReader().GetConfiguration(id);
            var ep = GetEndpoint(item, id, eid);
            var epb = from e in ep.Endpoints where e.Id == ebid select e;
            return View(epb.First());
        }

        [HttpPost]
        public ActionResult DeleteEndpointBinding(string id, int eid, int ebid, Endpoint model)
        {
            var item = GetConfigurationReader().GetConfiguration(id);
            var ep = GetEndpoint(item, id, eid);
            ep.Endpoints[ebid].Deleted = true;
            GetConfigurationReader().WriteConfigurationSet(item, id);
            return RedirectToAction("EditEndpoint", new { id, eid });
        }

        private static IConfigurationReader GetConfigurationReader()
        {
            var reader = Resolver.Resolve<IConfigurationReader>().Activate(Scope.Singleton);
            return reader;
        }

        private static EndpointConfig GetEndpoint(ConfigurationSet item, string id, int eid)
        {
            var endpoint = (from e in item.Endpoints where e.Id == eid select e).First();
            return endpoint;
        }
    }
}