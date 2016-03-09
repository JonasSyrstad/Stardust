using System;
using System.Linq;
using System.Web.Mvc;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Repository;
using Stardust.Starterkit.Configuration.Web.Models;
using Stardust.Wormhole;
using Environment = Stardust.Starterkit.Configuration.Repository.Environment;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    [Authorize]
    public class EnvironmentController : BaseController
    {
        private readonly IEnvironmentTasks reader;

        private readonly IConfigSetTask configSetTasks;

        public EnvironmentController(IEnvironmentTasks environmentTasks, IConfigSetTask configSetTasks)
        {
            this.reader = environmentTasks;
            this.configSetTasks = configSetTasks;
        }

        public ActionResult Details(string id, string item)
        {
            
            var env = reader.GetEnvironment(item);
            ViewBag.Trail = env.GetTrail();
            if (!env.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Name = env.ConfigSet.Name;
            ViewBag.System = env.ConfigSet.System;
            ViewBag.EnvironmentId = env.Id;
            return View(reader.GetEnvironment(item));
        }

        [HttpPost]
        public ActionResult Details(string id, string item, Environment model)
        {
            var env = reader.GetEnvironment(item);
            ViewBag.Trail = env.GetTrail();
            if (!env.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Name = env.ConfigSet.Name;
            ViewBag.System = env.ConfigSet.System;
            ViewBag.EnvironmentId = env.Id;
            env.Description = model.Description;
            reader.UpdateEnvironment(env);
            return View(env);
        }

        public ActionResult Create(string id, string item)
        {
            var cs = configSetTasks.GetConfigSet(item);
            ViewBag.Trail = cs.GetTrail();
            if (!cs.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Name = cs.Name;
            ViewBag.System = cs.System;
            return View((object)null);
        }

        [HttpPost]
        public ActionResult Create(string id, string item, Environment model)
        {
            
            var cs = configSetTasks.GetConfigSet(item);
            if (!cs.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            var env = reader.CreatEnvironment(cs, model.Name);
            ViewBag.Name = cs.Name;
            ViewBag.System = cs.System;
            return RedirectToAction("Details", new { id = "edit", item = env.Id });
        }

        [HttpGet]
        public ActionResult Caching(string id, string item)
        {
            ViewBag.Id = item;
            var env = reader.GetEnvironment(item);
            ViewBag.Trail = env.GetTrail();
            return View(env.CacheType);
        }

        [HttpPost]
        public ActionResult Caching(string id, string item, CacheSettings model)
        {
            var env = reader.GetEnvironment(item);
            //env.CacheType.CacheImplementation = model.CacheImplementation;
            //env.CacheType.Secure = model.Secure;
            //env.CacheType.MachineNames = model.MachineNames;
            //env.CacheType.Port = model.Port;
            //env.CacheType.PassPhrase = model.PassPhrase;
            //env.CacheType.Port = model.Port;
            //env.CacheType.CacheName = model.CacheName;
            model.Map().To(env.CacheType);
            reader.UpdateCacheSettingsParameter(env.CacheType);
            return RedirectToAction("Details", new { id = "edit", item = env.Id });
        }

        [HttpGet]
        public ActionResult Delete(string id, string item)
        {
            var env = reader.GetEnvironment(item);
            if (!env.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Name = env.ConfigSet.Name;
            ViewBag.Trail = env.GetTrail();
            ViewBag.System = env.ConfigSet.System;
            ViewBag.EnvironmentId = env.Id;
            return View(reader.GetEnvironment(item));
        }

        [HttpPost]
        public ActionResult Delete(string id, string item, Environment model)
        {
            var env = reader.GetEnvironment(item);
            reader.DeleteEnvironment(env);
            if (!env.UserHasAccessTo()) throw new UnauthorizedAccessException("Access denied to configset");
            ViewBag.Name = env.ConfigSet.Name;
            ViewBag.System = env.ConfigSet.System;
            ViewBag.EnvironmentId = env.Id;
            return RedirectToAction("Details", "ConfigSet", new { name = ViewBag.Name, system = ViewBag.System });
        }

        [HttpGet]
        public ActionResult FederationSettings(string id)
        {
            var env = reader.GetEnvironment(id);
            ViewBag.Name = env.ConfigSet.Name;
            ViewBag.System = env.ConfigSet.System;
            ViewBag.EnvironmentId = env.Id;
            ViewBag.Trail = env.GetTrail();
            var thumbprint = env.SubstitutionParameters.SingleOrDefault(p => p.Name == "Thumbprint");
            var federationServerHost = env.SubstitutionParameters.SingleOrDefault(p => p.Name == "IssuerAddress");
            return View(new FederationMeta { IssuerHostName = federationServerHost.Value, Thumbprint = thumbprint.Value });
        }

        [HttpPost]
        public ActionResult FederationSettings(string id, FederationMeta model)
        {
            var env = reader.GetEnvironment(id);
            ViewBag.Name = env.ConfigSet.Name;
            ViewBag.System = env.ConfigSet.System;
            ViewBag.EnvironmentId = env.Id;
            ViewBag.Trail = env.GetTrail();
            env.SubstitutionParameters.Single(p => p.Name == "Thumbprint").ItemValue = model.Thumbprint;
            env.SubstitutionParameters.Single(p => p.Name == "IssuerAddress").ItemValue = model.IssuerHostName;
            env.SubstitutionParameters.Single(p => p.Name == "IssuerActAsAddress").ItemValue = model.IssuerHostName;
            env.SubstitutionParameters.Single(p => p.Name == "IssuerMetadataAddress").ItemValue = model.IssuerHostName;
            env.SubstitutionParameters.Single(p => p.Name == "IssuerName").ItemValue = "http://" + model.IssuerHostName + "/adfs/services/trust";
            env.SubstitutionParameters.Single(p => p.Name == "StsAddress").ItemValue = "https://" + model.IssuerHostName + "/adfs/ls/";
            if (env.SubstitutionParameters.Single(p => p.Name == "CertificateValidationMode").ItemValue.IsNullOrWhiteSpace())
                env.SubstitutionParameters.Single(p => p.Name == "CertificateValidationMode").ItemValue = "None";
            if (env.SubstitutionParameters.Single(p => p.Name == "EnforceCertificateValidation").ItemValue.IsNullOrWhiteSpace())
                env.SubstitutionParameters.Single(p => p.Name == "EnforceCertificateValidation").ItemValue = "true";
            if (env.SubstitutionParameters.Single(p => p.Name == "RequireHttps").ItemValue.IsNullOrWhiteSpace())
                env.SubstitutionParameters.Single(p => p.Name == "RequireHttps").ItemValue = "true";
            reader.UpdateEnvironment(env);
            return RedirectToAction("Details", new { id = "edit", item = env.Id });
        }

        [HttpGet]
        public ActionResult ReaderKey(string id)
        {
            var env = reader.GetEnvironment(id);
            ViewBag.Name = env.ConfigSet.Name;
            ViewBag.System = env.ConfigSet.System;
            ViewBag.EnvironmentId = env.Id;
            ViewBag.Trail = env.GetTrail();
            return View(new ReaderKey{Key=env.GetReaderKey()});
        }

        [HttpPost]
        public ActionResult ReaderKey(string id,string model)
        {
            var env = reader.GetEnvironment(id);
            ViewBag.Name = env.ConfigSet.Name;
            ViewBag.System = env.ConfigSet.System;
            ViewBag.EnvironmentId = env.Id;
            ViewBag.Trail = env.GetTrail();
            reader.GenerateReaderKey(env.ConfigSet.Id, env.Name);
            return View(new ReaderKey { Key = env.GetReaderKey() });
        }

        [HttpGet]
        public ActionResult NotifyChange(string id)
        {
            var env = reader.GetEnvironment(id);
            ViewBag.Name = env.ConfigSet.Name;
            ViewBag.System = env.ConfigSet.System;
            ViewBag.EnvironmentId = env.Id;
            ViewBag.Trail = env.GetTrail();
            return View(new EnvironmentOverview{Name=env.Name, Id=env.Id});
        }

        [HttpPost]
        public ActionResult NotifyChange(string id,EnvironmentOverview model)
        {
            try
            {
                var env = reader.GetEnvironment(id);
                ViewBag.Name = env.ConfigSet.Name;
                ViewBag.System = env.ConfigSet.System;
                ViewBag.EnvironmentId = env.Id;
                ViewBag.Trail = env.GetTrail();
                reader.PushChange(env.ConfigSet.Id, env.Name);
                return RedirectToAction("Details", "ConfigSet", new { name = env.ConfigSet.Name, system = env.ConfigSet.System });
            }
            catch (Exception ex)
            {
                ex.Log();
                throw;
            }
        }
    }

    public class EnvironmentOverview 
    {
        public string Name { get; set; }

        public string Id { get; set; }
    }
}