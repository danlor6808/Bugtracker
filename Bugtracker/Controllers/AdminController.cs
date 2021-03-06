﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bugtracker.Models;
using Microsoft.AspNet.Identity;

namespace Bugtracker.Controllers
{
    [RequireHttps]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ManageUsers()
        {
            var users = new List<UserViewModel>();
            var helper = new UserRolesHelper(db);
            foreach (var user in db.Users.ToList())
            {
                var eachUser = new UserViewModel();
                eachUser.roles = new List<string>();
                eachUser.user = user;
                eachUser.roles = helper.ListUserRoles(user.Id).ToList();

                users.Add(eachUser);
            }
            return View(users);
        }

        // GET:
        [Authorize(Roles = "Administrator")]
        public ActionResult EditUser(string id)
        {
            var user = db.Users.Find(id);
            var model = new AdminViewModel();
            var helper = new UserRolesHelper(db);
            model.Id = user.Id;
            model.Name = user.UserName;
            model.SelectedRoles = helper.ListUserRoles(id).ToArray();
            model.Roles = new MultiSelectList(db.Roles, "Name", "Name", model.SelectedRoles);

            return View(model);
        }

        // POST
        [HttpPost]
        public ActionResult EditUser(AdminViewModel model)
        {
            var helper = new UserRolesHelper(db);
            var roleList = helper.ListUserRoles(model.Id);
            var newroleList = model.SelectedRoles;
            foreach (var role in roleList)
            {
                helper.RemoveUserFromRole(model.Id, role);
            }
            if (newroleList != null)
            {
                foreach (var role in newroleList)
                {
                    helper.AddUserToRole(model.Id, role);
                }
            }
            return RedirectToAction("ManageUsers");
        }
    }
}