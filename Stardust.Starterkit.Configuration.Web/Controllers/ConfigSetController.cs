using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;
using Stardust.Particles.Xml;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Repository;
using Stardust.Starterkit.Configuration.Web.Models;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    [Authorize]
    public class ConfigSetController : BaseController
    {


        public ActionResult Details(string name, string system)
        {
            var cs = ConfigReaderFactory.GetConfigSetTask().GetConfigSet(name, system);
            if (!cs.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Id = cs.Id;
            return View(cs);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Details(string name, string system, ConfigSet model)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var cs = reader.GetConfigSet(name, system);
            if (!cs.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            cs.Description = model.Description;
            cs.LayerNames = model.LayerNames;
            reader.UpdateConfigSet(cs);
            ViewBag.Id = cs.Id;
            return View(cs);
        }

        [Authorize]
        public ActionResult Create(string id1, string id2)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            IConfigSet parent = null;

            if (id1.ContainsCharacters())
                parent = reader.GetConfigSet(id1, id2);
            if (parent == null)
            {
                if (ConfigReaderFactory.CurrentUser.AdministratorType != AdministratorTypes.SystemAdmin) throw new UnauthorizedAccessException("Access denied to configset");
                ViewBag.IsNew = true;
                ViewBag.CreateType = "new";
            }
            else
            {
                if (!parent.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
                ViewBag.IsNew = false;
                ViewBag.CreateType = string.Format("child for {0}.{1}", id1, id2);
            }
            return View(parent);
        }


        [Authorize]
        [HttpPost]
        public ActionResult Create(string id1, string id2, ConfigSet model)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            IConfigSet parent = null;
            if (id1.ContainsCharacters())
            {
                parent = reader.GetConfigSet(id1, id2);
                if (!parent.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            }
            else
            {
                if (ConfigReaderFactory.CurrentUser.AdministratorType != AdministratorTypes.SystemAdmin) throw new UnauthorizedAccessException("Access denied to configset");
            }
            reader.CreateConfigSet(model.Name, string.IsNullOrEmpty(model.System) ? id2 : model.System, parent);
            var csId = reader.GetConfigSet(model.Name, string.IsNullOrEmpty(model.System) ? id2 : model.System);
            return RedirectToAction("Details", "ConfigSet", new { name = csId.Name, system = csId.System });
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            var data = Deserializer<ConfigurationSets>.Deserialize(file.InputStream, "http://pragma.no/Pragma.Core.Services.ConfigurationReader.ConfigurationSets");
            var reader = ConfigReaderFactory.GetConfigSetTask();
            foreach (var configSet in data.ConfigSets)
            {
                reader.CreateFromTextConfigStoreFile(configSet);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Overview(string name, string system, string env)
        {
            var cs = ConfigReaderFactory.GetConfigSetTask().GetConfigSet(name, system);
            if (!cs.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Trail = cs.GetTrail();
            ViewBag.Id = cs.Id;
            ViewBag.Env = env;
            ViewBag.System = cs.System;
            ViewBag.Name = cs.Name;
            var envs = cs.Environments.Select(s => s.Name).ToList();
            envs.Insert(0, "");
            ViewBag.Environments = envs;
            return View(cs);
        }

        [HttpGet]
        public ActionResult ReaderKey(string id)
        {
            var cs = ConfigReaderFactory.GetConfigSetTask().GetConfigSet(id);
            if (!cs.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Trail = cs.GetTrail();
            ViewBag.Id = cs.Id;
            ViewBag.System = cs.System;
            ViewBag.Name = cs.Name;
            return View(new ReaderKey { Key = cs.ReaderKey, AllowMaster = cs.AllowAccessWithRootKey });
        }
        [HttpPost]
        public ActionResult ReaderKey(string id, ReaderKey model)
        {
            try
            {
                var reader = ConfigReaderFactory.GetConfigSetTask();
                var cs = reader.GetConfigSet(id);
                if (!cs.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
                ViewBag.Trail = cs.GetTrail();
                ViewBag.Id = cs.Id;
                ViewBag.System = cs.System;
                ViewBag.Name = cs.Name;
                cs.AllowAccessWithRootKey = model.AllowMaster;
                if (model.GenerateNew)
                    reader.GenerateReaderKey(cs);
                reader.UpdateConfigSet(cs);
                return RedirectToAction("ReaderKey", new { id = id });
            }
            catch (Exception exception)
            {
                exception.Log();
                throw exception;
            }
        }
    }
}
