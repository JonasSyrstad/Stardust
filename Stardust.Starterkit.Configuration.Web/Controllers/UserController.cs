using System;
using System.Linq;
using System.Web.Mvc;
using Stardust.Interstellar;
using Stardust.Interstellar.Utilities;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Repository;
using Stardust.Starterkit.Configuration.Web.Models;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    
    public class UserController : BaseController
    {
        private readonly IUserFacade userReader;

        private readonly IConfigSetTask reader;

        public UserController(IRuntime runtime,IUserFacade userReader, IConfigSetTask reader):base(runtime)
        {
            this.userReader = userReader;
            this.reader = reader;
        }

        // GET: User
        [Authorize()]
        public ActionResult Index()
        {
            return View(userReader.GetUsers());
        }
        [Authorize(Roles = "SystemAdmin")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "SystemAdmin")]
        public ActionResult Create(ConfigUser model)
        {
            userReader.CreateUser(model);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(string id)
        {
            id = Server.UrlDecode(id);
            var user = userReader.GetUser(id);
            ValidateAccess(user);
            return View(user);
        }

        private void ValidateAccess(IConfigUser user)
        {
            
            if (!String.Equals(user.NameId, ConfigReaderFactory.CurrentUser.NameId, StringComparison.OrdinalIgnoreCase) && ConfigReaderFactory.CurrentUser.AdministratorType != AdministratorTypes.SystemAdmin)
            {
                throw new UnauthorizedAccessException("Forbidden");
            }
        }

        [HttpPost]
        public ActionResult Edit(string id, ConfigUser model)
        {
            id = Server.UrlDecode(id);
            var user = userReader.GetUser(id);
            ValidateAccess(user);
            userReader.UpdateUser(model);
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "SystemAdmin")]
        public ActionResult AddToSet(string id)
        {
            var configSet = reader.GetConfigSet(id);
            ViewBag.Id = id;
            return View(new AdministratorsModel{CurrentAdministrators = configSet.Administrators.ToList(),AvaliableUsers = ConfigReaderFactory.GetUserFacade().GetUsers().ToList(), PostedUserIds = new string[]{}});
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost]
        public ActionResult AddToSet(string id, AdministratorsModel model)
        {
            var configSet = reader.GetConfigSet(id);
            var userList = configSet.Administrators.ToList();
            foreach (var userId in model.PostedUserIds)
            {
                var user = ConfigReaderFactory.GetUserFacade().GetUser(userId);
                if (!(from u in userList where u.NameId == user.NameId select u).Any())
                {
                    configSet.Administrators.Add(user);
                }
            }
            var usersToRemove = configSet.Administrators.Where(administrator => !model.PostedUserIds.Contains(administrator.NameId)).ToList();
            foreach (var configUser in usersToRemove)
            {
                configSet.Administrators.Remove(configUser);
            }
            reader.UpdateAdministrators(configSet.Administrators);
            userReader.SendNotifications(configSet.Administrators, usersToRemove);
            ViewBag.Id = id;
            return RedirectToAction("Details","ConfigSet", new { name = configSet.Name, system = configSet.System });
        }

        [HttpGet]
        [Authorize()]
        public ActionResult AccessToken(string id)
        {
            id = Server.UrlDecode(id);
            var user = userReader.GetUser(id);
            ValidateAccess(user);
            ViewBag.UserId = id;
            return View(new ReaderKey { Key = user.GetAccessToken() });
        }

        [HttpPost]
        [Authorize()]
        public ActionResult AccessToken(string id, string model)
        {
            id = Server.UrlDecode(id);
            var user = userReader.GetUser(id);
            ValidateAccess(user);
            userReader.GenerateAccessToken(id);
            ViewBag.UserId = id;
            return View(new ReaderKey { Key = user.GetAccessToken() });
        }

        [Authorize(Roles = "SystemAdmin")]
        public ActionResult Delete(string id)
        {
            id = Server.UrlDecode(id);
            var user = userReader.GetUser(id);
            ValidateAccess(user);
            ViewBag.UserId = id;
            return View(user);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost]
        public ActionResult Delete(string id, ConfigUser model)
        {
            id = Server.UrlDecode(id);
            var user = userReader.GetUser(id);
            ValidateAccess(user);
            userReader.DeleteUser(user);
            ViewBag.UserId = id;
            return RedirectToAction("Index");
        }
    }
}