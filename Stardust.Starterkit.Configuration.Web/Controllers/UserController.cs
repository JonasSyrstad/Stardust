using System.Linq;
using System.Web.Mvc;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Repository;
using Stardust.Starterkit.Configuration.Web.Models;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    [Authorize(Roles = "SystemAdmin")]
    public class UserController : BaseController
    {
        // GET: User
        public ActionResult Index()
        {
            var userReader = ConfigReaderFactory.GetUserFacade();
            return View(userReader.GetUsers());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(ConfigUser model)
        {
            var userReader = ConfigReaderFactory.GetUserFacade();
            userReader.CreateUser( model)
            ;
            return RedirectToAction("Index");
        }

        public ActionResult Edit(string id)
        {
            var userReader = ConfigReaderFactory.GetUserFacade();
            return View(userReader.GetUser(id));
        }
        [HttpPost]
        public ActionResult Edit(string id, ConfigUser model)
        {
            var userReader = ConfigReaderFactory.GetUserFacade();
            userReader.UpdateUser(model);
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "SystemAdmin")]
        public ActionResult AddToSet(string id)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var configSet = reader.GetConfigSet(id);
            ViewBag.Id = id;
            return View(new AdministratorsModel{CurrentAdministrators = configSet.Administrators.ToList(),AvaliableUsers = ConfigReaderFactory.GetUserFacade().GetUsers().ToList(), PostedUserIds = new string[]{}});
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost]
        public ActionResult AddToSet(string id, AdministratorsModel model)
        {
            var reader = ConfigReaderFactory.GetConfigSetTask();
            var configSet = reader.GetConfigSet(id);
            var userList = configSet.Administrators.ToList();
            foreach (var userId in model.PostedUserIds)
            {
                var user = ConfigReaderFactory.GetUserFacade().GetUser(userId);
                if(!(from u in userList where u.NameId==user.NameId select u).Any())
                    configSet.Administrators.Add(user);
            }
            var usersToRemove = configSet.Administrators.Where(administrator => !model.PostedUserIds.Contains(administrator.NameId)).ToList();
            foreach (var configUser in usersToRemove)
            {
                configSet.Administrators.Remove(configUser);
            }
            reader.UpdateAdministrators(configSet.Administrators);
            ViewBag.Id = id;
            return RedirectToAction("Details","ConfigSet", new { name = configSet.Name, system = configSet.System });
        }
    }
}