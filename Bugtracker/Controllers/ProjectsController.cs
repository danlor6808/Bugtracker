using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Bugtracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Bugtracker.Controllers
{
    [Authorize]
    [RequireHttps]
    public class ProjectsController : MyBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        

        // GET: Projects
        public ActionResult Index(string view)
        {
            ViewBag.Header = "Projects";
            ViewBag.layout = "sidebar-mini sidebar-collapse fixed";
            ViewBag.Current = "Projects";
            var list = new List<Project>();
            if (view == "all")
            {
                list = db.Project.ToList();
                return View(list);
            }
            if (User.IsInRole("Administrator"))
            {
                list = db.Project.ToList();
            }
            else
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                list = user.Projects.ToList();
                //list = db.Project.Where(u => u.User.Any(t => t.Id == user.Id)).ToList();
            }
            return View(list);
        }

        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
            ViewBag.Header = "Details";
            ViewBag.layout = "sidebar-mini sidebar-collapse fixed";
            ViewBag.Current = "Projects";
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Project.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Administrator")]
        public ActionResult Create()
        {
            ViewBag.Header = "Create a project";
            ViewBag.layout = "sidebar-mini sidebar-collapse fixed";
            ViewBag.Current = "Projects";
            var pmId = db.Roles.First(u => u.Name == "Project Manager");
            //List of all users that are project managers
            ViewBag.userList = db.Users.Where(u => u.Roles.Any(r => r.RoleId == pmId.Id));
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create([Bind(Include = "Id,Name,Deadline")] Project project, string UserSelected)
        {
            if (ModelState.IsValid)
            {
                if(db.Project.Any(p => p.Name == project.Name))
                {
                    ModelState.AddModelError("Name", "The title must be unique.");
                    return View(project);
                }
                project.Created = System.DateTimeOffset.Now;
                project.Status = "Open";
                project.ProjectManagerId = UserSelected;
                db.Project.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Edit(int? id)
        {
            ViewBag.Header = "Edit";
            ViewBag.layout = "sidebar-mini sidebar-collapse fixed";
            ViewBag.Current = "Projects";
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Project.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }

            ProjectUserHelper helper = new ProjectUserHelper(db);
            //Status list
            ViewBag.Status = helper.statuslist(project.Id);
            var pmId= db.Roles.First(u => u.Name == "Project Manager");
            //List of all users that are project managers
            ViewBag.userList = db.Users.Where(u => u.Roles.Any(r => r.RoleId == pmId.Id));

            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Project Manager")]
        public ActionResult Edit([Bind(Include = "Id,Name,Updated,Status,Deadline")] Project project, string UserSelected)
        {
            if (ModelState.IsValid)
            {
                var currentProject = db.Project.Find(project.Id);
                var allUsers = db.Users.ToList();
                var helper = new ProjectUserHelper(db);
                currentProject.Updated = DateTimeOffset.Now;
                currentProject.Name = project.Name;
                currentProject.Status = project.Status;
                currentProject.Deadline = project.Deadline;
                if (UserSelected != null)
                {
                    currentProject.ProjectManagerId = UserSelected;
                } else
                {
                    currentProject.ProjectManagerId = Request["ProjectManagerId"];
                }

                var pmId = db.Roles.First(u => u.Name == "Project Manager");

                //Checks if Project name is empty and throws an error if true
                if (String.IsNullOrWhiteSpace(project.Name))
                {
                    ModelState.AddModelError("Name", "The title must be unique.");

                    //Status list
                    ViewBag.Status = helper.statuslist(project.Id);
                    //List of all users that are project managers
                    ViewBag.userList = db.Users.Where(u => u.Roles.Any(r => r.RoleId == pmId.Id));
                    return View(project);
                }

                //
                var ProjectAlreadyExists = db.Project.Where(p => p.Id == project.Id && p.Name == project.Name).Select(p => p.Name);
                if (!ProjectAlreadyExists.Any())
                {
                    if (db.Project.Any(p => p.Name == project.Name))
                    {
                        ModelState.AddModelError("Name", "The title must be unique.");

                        //Status list
                        ViewBag.Status = helper.statuslist(project.Id);
                        //List of all users that are project managers
                        ViewBag.userList = db.Users.Where(u => u.Roles.Any(r => r.RoleId == pmId.Id));
                        return View(project);
                    }
                }

                //save changes
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int? id)
        {
            ViewBag.Header = "Delete";
            ViewBag.layout = "sidebar-mini sidebar-collapse fixed";
            ViewBag.Current = "Projects";
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Project.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            db.Project.Remove(project);
            db.SaveChanges();
            ViewBag.StatusMessage = "Project has been successfully deleted";
            return RedirectToAction("Index", "Projects");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) 
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //Create Project Partial
        // GET: Projects/Create
        [Authorize(Roles = "Administrator")]
        public ActionResult CreateProjectPartial()
        {
            ViewBag.Header = "Create a project";
            ViewBag.layout = "sidebar-mini";
            var pmId = db.Roles.First(u => u.Name == "Project Manager");
            //List of all users that are project managers
            ViewBag.userList = db.Users.Where(u => u.Roles.Any(r => r.RoleId == pmId.Id));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult CreateProjectPartial([Bind(Include = "Id,Name,Deadline")] Project project, string UserSelected)
        {
            if (ModelState.IsValid)
            {
                if (db.Project.Any(p => p.Name == project.Name))
                {
                    ModelState.AddModelError("Name", "The title must be unique.");
                    return View(project);
                }
                project.Created = System.DateTimeOffset.Now;
                project.Status = "Open";
                project.ProjectManagerId = UserSelected;
                db.Project.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }
    }
}
