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

namespace Bugtracker.Controllers
{
    [Authorize]
    [RequireHttps]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var project = db.Project.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            ViewBag.projId = project.Id;
            //var ticket = db.Ticket.Include(t => t.AssignedUser).Include(t => t.Author).Include(t => t.Project);
            return View(project.Tickets.ToList());
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Ticket.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var project = db.Project.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            Ticket ticket = new Ticket();
            ticket.Project = project;
            ticket.ProjectId = project.Id;
            ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type");
            ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name");
            return View(ticket);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ProjectId,Title,Body,StatusId,TicketTypeId,PriorityId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Created = DateTimeOffset.Now;
                ticket.AuthorId = User.Identity.GetUserId();
                ticket.Project = db.Project.Find(ticket.ProjectId);
                ticket.StatusId = 1;

                if (db.Ticket.Any(p => p.Title == ticket.Title && p.ProjectId == ticket.ProjectId))
                {
                    ModelState.AddModelError("Title", "The title must be unique.");
                    ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type", ticket.TicketTypeId);
                    ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name", ticket.PriorityId);
                    return View(ticket);
                }

                db.Ticket.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index", new { id = ticket.ProjectId});
            }
            ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type", ticket.TicketTypeId);
            ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name", ticket.PriorityId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Ticket.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.AssignedUser = new SelectList(db.Users, "Id", "UserName");
            ViewBag.TicketStatus = new SelectList(db.TicketStatus, "Id", "Name", ticket.StatusId);
            ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type", ticket.TicketTypeId);
            ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name", ticket.PriorityId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ProjectId,Title,Body,StatusId,TicketTypeId,PriorityId,AssignedUserId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                //db.Entry(ticket).State = EntityState.Modified;
                db.Ticket.Attach(ticket);
                db.Entry(ticket).Property("Body").IsModified = true;
                db.Entry(ticket).Property("Title").IsModified = true;
                db.Entry(ticket).Property("StatusId").IsModified = true;
                db.Entry(ticket).Property("TicketTypeId").IsModified = true;
                db.Entry(ticket).Property("PriorityId").IsModified = true;
                db.Entry(ticket).Property("AssignedUserId").IsModified = true;

                if (String.IsNullOrWhiteSpace(ticket.Title))
                {
                    ModelState.AddModelError("Title", "Please enter a valid title.");

                    ViewBag.AssignedUser = new SelectList(db.Users, "Id", "UserName", ticket.AssignedUserId);
                    ViewBag.TicketStatus = new SelectList(db.TicketStatus, "Id", "Name", ticket.StatusId);
                    ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type", ticket.TicketTypeId);
                    ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name", ticket.PriorityId);
                    return View(ticket);
                }

                var TicketAlreadyExists = db.Ticket.Where(p => p.Id == ticket.Id && p.Title == ticket.Title).Select(p => p.Title);
                if (!TicketAlreadyExists.Any())
                {
                    if (db.Ticket.Any(p => p.Title == ticket.Title && p.ProjectId == ticket.ProjectId))
                    {
                        ModelState.AddModelError("Title", "The title must be unique.");

                        ViewBag.AssignedUser = new SelectList(db.Users, "Id", "UserName", ticket.AssignedUserId);
                        ViewBag.TicketStatus = new SelectList(db.TicketStatus, "Id", "Name", ticket.StatusId);
                        ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type", ticket.TicketTypeId);
                        ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name", ticket.PriorityId);
                        return View(ticket);
                    }
                }
            ticket.Updated = DateTimeOffset.Now;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = ticket.ProjectId });
            }
            ViewBag.AssignedUser = new SelectList(db.Users, "Id", "UserName", ticket.AssignedUserId);
            ViewBag.TicketStatus = new SelectList(db.TicketStatus, "Id", "Name", ticket.StatusId);
            ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type", ticket.TicketTypeId);
            ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name", ticket.PriorityId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Ticket.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Ticket.Find(id);
            db.Ticket.Remove(ticket);
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
