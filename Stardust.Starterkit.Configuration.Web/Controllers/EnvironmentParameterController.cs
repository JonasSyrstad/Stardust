using System;
using System.IO;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Business;
using System.Web.Mvc;
using Stardust.Starterkit.Configuration.Repository;
using Stardust.Starterkit.Configuration.Web.Models;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    [Authorize]
    public class EnvironmentParameterController : BaseController
    {
        public ActionResult EditSub(string id, string item)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var subPar = reader.GetSubstitutionParameter(item);
            ViewBag.Trail = subPar.GetTrail();
            if (!subPar.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Id = subPar.Environment.Id;
            return View(subPar);
        }

        [HttpPost]
        public ActionResult EditSub(string id, string item, SubstitutionParameter model)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var subPar = reader.GetSubstitutionParameter(item);
            subPar.IsSecure = model.IsSecure;   
            subPar.ItemValue = model.ItemValue.IsInstance() ? model.ItemValue.TrimEnd() : null;
            if (subPar.IsSecure && subPar.ItemValue.ContainsCharacters()) subPar.ItemValue = subPar.ItemValue.Encrypt(KeyHelper.SharedSecret);
            if (!subPar.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            reader.UpdateSubstitutionParameter(subPar);
            return RedirectToAction("Details", "Environment", new { id = "edit", item = subPar.Environment.Id });
        }

        public ActionResult DeleteSub(string id, string env, string sub)
        {
            var subPar = ConfigReaderFactory.GetConfigSetTask().GetSubstitutionParameter(sub);
            ViewBag.Trail = subPar.GetTrail();
            return View(subPar);
        }

        [HttpPost]
        public ActionResult DeleteSub(string id, string env, string sub,SubstitutionParameter model)
        {
            var subParam = ConfigReaderFactory.GetConfigSetTask().GetSubstitutionParameter(sub);
            if (subParam.Id != sub) throw new InvalidDataException("Substitution parameters don't match");
            ConfigReaderFactory.GetConfigSetTask().DeleteSubstitutionParameter(env,sub);
            return RedirectToAction("Details", "Environment", new { id = "edit", item = env });
        }

        

        public ActionResult Create(string id, string item)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var env = reader.GetEnvironment(item);
            ViewBag.Trail = env.GetTrail();
            if (!env.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.EnvironmentId = env.Id;
            return View((object)null);
        }

        [HttpPost]
        public ActionResult Create(string id, string item, EnvironmentParameter model)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var env = reader.GetEnvironment(item);
            if (!env.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            reader.CreatEnvironmentParameter(env, model.Name, model.ItemValue.TrimEnd(), model.IsSecureString);
            ViewBag.EnvironmentId = env.Id;
            return RedirectToAction("Details", "Environment", new { id = "edit", item = env.Id });
        }

        public ActionResult Edit(string id, string item)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var par = reader.GetEnvironmentParameter(item);
            ViewBag.Trail = par.GetTrail();
            if (!par.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Id = par.Environment.Id;
            return View(par);
        }

        [HttpPost]
        public ActionResult Edit(string id, string item, EnvironmentParameter model)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var par = reader.GetEnvironmentParameter(item);
            if (!par.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            par.IsSecureString = model.IsSecureString;
            if (model.ItemValue != par.ItemValue)
            {
                if (model.ItemValue.IsNullOrWhiteSpace()) par.ItemValue = null;
                else
                {
                    par.SetValue(model.ItemValue.TrimEnd());
                }
            }
            reader.UpdateEnvironmentParameter(par);
            return RedirectToAction("Details", "Environment", new { id = "edit", item = par.Environment.Id });
        }
    }
}