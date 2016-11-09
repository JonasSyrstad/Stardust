using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Stardust.Interstellar;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Business;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    [Authorize(Roles = "SystemAdmin")]
    public class MigrationController : BaseController
    {
        // GET: Migration
        public ActionResult Index()
        {
            ViewBag.CurentConfigDatabase = RepositoryFactory.GetConnectionString().Split(';').Last().Split('=').Last();
            var stores = Directory.EnumerateDirectories(ConfigurationManagerHelper.GetValueOnKey("stardust.StoreLocation"))
                .Select(d => d.Split('\\').LastOrDefault());
            return View(stores);
        }

        public ActionResult OverWrite(string id)
        {
            var tempDir = ConfigurationManagerHelper.GetValueOnKey("stardust.StoreLocation") + "\\" + id;
            ;
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            return Content("Ok");
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {

            try
            {
                var fileName = string.Format("{0}\\{1}.bbs", ConfigurationManagerHelper.GetValueOnKey("stardust.StoreLocation"), DateTime.Now.Ticks);
                file.SaveAs(fileName);
                RepositoryFactory.Restore(fileName);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            Thread.Sleep(100);
            var stores = Directory.EnumerateDirectories(ConfigurationManagerHelper.GetValueOnKey("stardust.StoreLocation")).Select(Path.GetDirectoryName);
            return RedirectToAction("Index");
        }

        public ActionResult Backup(string id)
        {
            try
            {
                var path = Path.Combine(ConfigurationManagerHelper.GetValueOnKey("stardust.StoreLocation"), id);
                var tempDir = RepositoryFactory.CreateTempDir(id, path);
                var file = string.Format("{0}\\{1}.zip", ConfigurationManagerHelper.GetValueOnKey("stardust.StoreLocation"), id);
                var finfo = RepositoryFactory.Backup(file, tempDir);
                return File(finfo.FullName, "application/zip, application/octet-stream", string.Format("{0}.zip", id));
            }
            catch(Exception ex)
            {
                ex.Log();
                var currentStore = RepositoryFactory.GetConnectionString().Split(';').Last().Split('=').Last();
                var lastFile = Directory.GetFiles(ConfigurationManagerHelper.GetValueOnKey("stardust.StoreLocation")).Where(f => f.Contains(currentStore) && !f.Contains( currentStore + ".zip")).OrderBy(s => s).SingleOrDefault();
                return File(lastFile, "application/zip, application/octet-stream", string.Format("{0}.zip", id));
            }
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Web.Mvc.Controller"/> class.
        /// </summary>
        public MigrationController(IRuntime runtime)
            : base(runtime)
        {
        }
    }
}