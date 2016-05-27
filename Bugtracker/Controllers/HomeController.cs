using Bugtracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Bugtracker.Controllers
{
    [RequireHttps]
    public class HomeController : MyBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            ViewBag.layout = "sidebar-collapse fixed";
            ViewBag.Current = "Home";
            return View();
        }

        [Authorize]
        public ActionResult UserPanel()
        {
            ViewBag.layout = "sidebar-mini sidebar-collapse fixed";
            ViewBag.Current = "Dashboard";
            var user = db.Users.Find(User.Identity.GetUserId());
            //Graph/Chart Details
            ViewBag.TotalProjects = db.Project.ToList();
            ViewBag.TotalTickets = db.Ticket.ToList();
            ViewBag.TotalUsers = db.Users.ToList();
            var totalTickets = db.Ticket.ToList();
            var completedTickets = db.Ticket.Where(u => u.StatusId == 3).ToList();
            ViewBag.CompleteTicketsStatic = completedTickets.Count;
            ViewBag.CompletedTickets = (int)Math.Round((double)(100 * completedTickets.Count) / totalTickets.Count);
            return View();
        }

        [Authorize]
        public ActionResult DismissNotification(int? tId, int? nId, bool da)
        {
            var user = User.Identity.GetUserId();
            var notification = db.Notification.Find(nId);
            var ticket = db.Ticket.Find(tId);

            //if action is passed without notificationID and ticketID, it will clear out all notification user has
            //as long as the variable "da" is true. da = "dismiss all" bool
            if (da && nId == null && tId == null)
            {
                var notes = db.Notification.Where(u => u.UserId == user);
                foreach (var note in notes)
                {
                    db.Notification.Remove(note);
                }
                db.SaveChanges();
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }

            //check if notification id is valid
            if (nId != null )
            {
                if (notification == null)
                {
                    return HttpNotFound();
                }
            }
            //Checks if notifications belong to you
            if (notification.UserId != user)
            {
                return HttpNotFound();
            }
            //check if ticket id is valid, if so it will delete notification and redirect to ticket
            if (tId != null)
            {
                if (ticket != null)
                {
                    db.Notification.Remove(notification);
                    db.SaveChanges();
                    return RedirectToAction("Details", "Tickets", new { id = tId });
                }
            }

            //gets here only if notificationid is valid, and there is no ticketid.
            db.Notification.Remove(notification);
            db.SaveChanges();
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

    }
}