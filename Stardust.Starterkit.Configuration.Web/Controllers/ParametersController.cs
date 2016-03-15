using Stardust.Particles;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Repository;
using Stardust.Starterkit.Configuration.Web.Models;
using System;
using System.Web.Mvc;
using Stardust.Interstellar;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    [Authorize]
    public class ParametersController : BaseController
    {
        private IConfigSetTask reader;

        public ParametersController(IConfigSetTask reader,IRuntime runtime):base(runtime)
        {
            this.reader = reader;
        }
        public ActionResult Edit(string id, string item)
        {
            var parameter = reader.GetEnpointParameter(item);
            ViewBag.Trail = parameter.GetTrail();
            if (!parameter.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            return View(parameter);
        }

        [HttpPost]
        public ActionResult Edit(string id, string item, EndpointParameter model)
        {
            var parameter = reader.GetEnpointParameter(item);
            ViewBag.Trail = parameter.GetTrail();
            if (!parameter.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            parameter.ItemValue = TrimParameter(model);
            reader.UpdateEndpointParameter(parameter);
            return View(parameter);
        }

        private static string TrimParameter(EndpointParameter model)
        {
            if (model.ItemValue.IsNullOrEmpty()) return null;
            return model.ItemValue.TrimEnd();
        }

        public ActionResult CreateSub(string id, string item)
        {
            var endpoint = reader.GetEndpoint(item);
            ViewBag.Trail = endpoint.GetTrail();
            return View();
        }

        [HttpPost]
        public ActionResult CreateSub(string id, string item, EndpointPropertyModel model)
        {
            var endpoint = reader.GetEndpoint(item);
            reader.CreateEndpointParameter(item, model.Name, model.ItemValue,model.IsSubstiturtionParameter);
            return RedirectToAction("Details", "Endpoint", new { id = "edit", item = item });
        }
    }
}