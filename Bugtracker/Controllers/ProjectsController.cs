using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Bugtracker.Models;

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
            //var Status = new List<SelectListItem>();
            //Status.Add(new SelectListItem { Text = "Open", Value = "Open" });
            //Status.Add(new SelectListItem { Text = "Closed", Value = "Closed" });
            //Status.Add(new SelectListItem { Text = "Work in Progress", Value = "Work in Progress" });

            var Status = new[]
            {
            new SelectListItem() { Text = "Open", Value = "Open" },
            new SelectListItem() { Text = "Closed", Value = "Closed" },
            new SelectListItem() { Text = "Work in Progress", Value = "Work in Progress" }
            };

            var SelectedStatus = Status.First(d => d.Value == project.Status);
            if (SelectedStatus != null)
            {
                SelectedStatus.Selected = true;
            }
            ViewBag.Status = Status;

            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit([Bind(Include = "Id,Name,Updated,Status,Deadline")] Project project)
        {
            if (ModelState.IsValid)
            {
                //db.Entry(project).State = EntityState.Modified;
                project.Updated = System.DateTimeOffset.Now;
                db.Project.Attach(project);
                db.Entry(project).Property("Name").IsModified = true;
                db.Entry(project).Property("Updated").IsModified = true;
                db.Entry(project).Property("Status").IsModified = true;
                db.Entry(project).Property("Deadline").IsModified = true;

                if (String.IsNullOrWhiteSpace(project.Name))
                {
                    ModelState.AddModelError("Name", "Invalid Title.");
                    return View(project);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        [ValidateAntiForgeryToken]
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
