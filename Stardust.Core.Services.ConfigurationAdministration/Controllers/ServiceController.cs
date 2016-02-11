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
    public class ServiceController : Controller
    {
        //
        // GET: /Environment/

        public ActionResult Index(string id)
        {
            ViewBag.SetName = id;
            var item = GetConfigurationReader().GetConfiguration(id);
            return View(item.Services);
        }

        public ActionResult CreateEnvironment(string id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateEnvironment(string id, ServiceConfig model)
        {
            var item = GetConfigurationReader().GetConfiguration(id);
            if ((from s in item.Services where s.ServiceName == model.ServiceName select s).Any()) throw new IndexOutOfRangeException(string.Format("Service with name {0} already exists", model.ServiceName));
            model.IdentitySettings = new IdentitySettings
                                     {
                                         Audiences = new List<string> { "test" },
                                         CertificateValidationMode = "None",
                                         IssuerAddress = "test",
                                         IssuerName = "test",
                                         Realm = "test",
                                         RequireHttps = false,
                                         Thumbprint = "then"
                                     };
            item.Services.Add(model);
            GetConfigurationReader().WriteConfigurationSet(item, id);
            return RedirectToAction("Index", new { id = id });
        }

        public ActionResult EditEnvironment(string id, string name)
        {
            var item = GetConfigurationReader().GetConfiguration(id);
            var env = (from e in item.Services where e.ServiceName == name select e).First();
            if (env.Parameters == null)
                env.Parameters = new List<ConfigParameter>();
            ViewBag.Id = id;
            return View(env);
        }

        [HttpPost]
        public ActionResult EditEnvironment(string id, string name, ServiceConfig model)
        {
            var item = GetConfigurationReader().GetConfiguration(id);
            var env = (from e in item.Services where e.ServiceName == name select e).First();
            env.ServiceName = model.ServiceName;
            env.Username = model.Username;
            if (!model.PasswordIsUnchanged)
                env.Password = model.Password;
            if (env.Parameters == null)
                env.Parameters = new List<ConfigParameter>();
            var keyName = Request.Form["keyName"];
            var value = Request.Form["keyValue"];
            if (keyName.ContainsCharacters())
            {
                var param = (from p in env.Parameters where p.Name == keyName select p).FirstOrDefault();
                if (param.IsNull())
                {
                    param = new ConfigParameter();
                    env.Parameters.Add(param);
                }
                param.Name = keyName;
                if (keyName.ToLower().Contains("password"))
                {
                    param.Value = value.Encrypt("mayTheKeysSupportAllMyValues");
                    param.BinaryValue = param.Value.GetByteArray();
                    if (param.Value.Decrypt("mayTheKeysSupportAllMyValues") != value) throw new StardustCoreException("Encryption validation failed!");
                }
                else
                    param.Value = value;
            }
            GetConfigurationReader().WriteConfigurationSet(item, id);
            ViewBag.Id = id;
            return RedirectToAction("EditEnvironment", new { id = id, name = model.ServiceName });
        }

        private static IConfigurationReader GetConfigurationReader()
        {
            var reader = Resolver.Resolve<IConfigurationReader>().Activate(Scope.Singleton);
            return reader;
        }
    }
}