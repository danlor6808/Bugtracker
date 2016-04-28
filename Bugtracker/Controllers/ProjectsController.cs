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
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        

        // GET: Projects
        public ActionResult Index()
        {
            return View(db.Project.ToList());
        }

        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
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
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create([Bind(Include = "Id,Name,Deadline")] Project project)
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
                db.Project.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int? id)
        {
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
            //List of all users
            ViewBag.userList = db.Users.ToList();
            //List of all projects that user current has
            ViewBag.selectList = project.User.Select(n => n.Id).ToList();

            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit([Bind(Include = "Id,Name,Updated,Status,Deadline")] Project project, List<string> UserSelected)
        {
            if (ModelState.IsValid)
            {
                var currentProject = db.Project.Find(project.Id);
                var allUsers = db.Users.ToList();
                var helper = new ProjectUserHelper(db);
                project.Updated = DateTimeOffset.Now;
                currentProject.User = project.User;
                currentProject.Name = project.Name;
                currentProject.Status = project.Status;
                currentProject.Deadline = project.Deadline;

                //Removes all users if there are any
                if (project.User != null)
                {
                    foreach (var user in allUsers)
                    {
                        if (db.Project.Find(project.Id).User.Any(u => u.Id == user.Id))
                        {
                            helper.removeProjectUser(project.Id, user.Id);
                        }
                    }
                }

                //Adds users if any are selected from the multiselect box
                if (UserSelected != null)
                {
                    foreach (var id in UserSelected)
                    {
                        helper.addProjectUser(project.Id, id);
                    }
                }

                //Checks if Project name is empty and throws an error if true
                if (String.IsNullOrWhiteSpace(project.Name))
                {
                    ModelState.AddModelError("Name", "The title must be unique.");

                    //Status list
                    ViewBag.Status = helper.statuslist(project.Id);
                    //List of all users
                    ViewBag.userList = db.Users.ToList();
                    //List of all projects that user current has
                    ViewBag.selectList = project.User.Select(n => n.Id).ToList();
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
                        //List of all users
                        ViewBag.userList = db.Users.ToList();
                        //List of all projects that user current has
                        ViewBag.selectList = project.User.Select(n => n.Id).ToList();
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

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Project.Find(id);
            db.Project.Remove(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
