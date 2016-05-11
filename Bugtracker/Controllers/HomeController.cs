using Bugtracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

        public ActionResult About()
        {

            return View();
        }

        public ActionResult Contact()
        {


            return View();
        }

        [Authorize]
        public ActionResult UserPanel()
        {
            ViewBag.layout = "sidebar-mini sidebar-collapse fixed";
            ViewBag.Header = "Dashboard";
            ViewBag.Current = "Dashboard";
            var user = db.Users.Find(User.Identity.GetUserId());
            var list = new List<Project>();
            if (User.IsInRole("Administrator"))
            {
                list = db.Project.OrderByDescending(o => o.Created).ToList();
                ViewBag.UserTickets = db.Ticket.OrderByDescending(d => d.Created).ToList();
            }
            else if (User.IsInRole("Project Manager"))
            {
                list = user.Projects.OrderByDescending(d => d.Created).ToList();
                var tickets = list.SelectMany(u => u.Tickets).ToList();
                tickets.AddRange(db.Ticket.Where(u => u.AuthorId == user.Id).ToList());
                tickets.AddRange(db.Ticket.Where(u => u.AssignedUserId == user.Id).ToList());
                ViewBag.UserTickets = tickets.OrderByDescending(d => d.Created).Distinct().ToList();
            }
            else
            { 
                var tempList = db.Ticket.Where(u => u.AuthorId == user.Id).ToList();
                tempList.AddRange(db.Ticket.Where(u => u.AssignedUserId == user.Id).ToList());
                ViewBag.UserTickets = tempList.OrderByDescending(d => d.Created).Distinct().ToList();
            }
            ViewBag.TotalProjects = db.Project.ToList();
            ViewBag.TotalTickets = db.Ticket.ToList();
            ViewBag.TotalUsers = db.Users.ToList();
            var totalTickets = db.Ticket.ToList();
            var completedTickets = db.Ticket.Where(u => u.StatusId == 3).ToList();
            ViewBag.CompletedTickets = (int)Math.Round((double)(100 * completedTickets.Count) / totalTickets.Count);
            return View(list);
        }
    }
}