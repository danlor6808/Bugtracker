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
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            ViewBag.layout = "sidebar-collapse";
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
            ViewBag.layout = "sidebar-mini";
            var user = db.Users.Find(User.Identity.GetUserId());
            var list = new List<Project>();
            if (User.IsInRole("Administrator")) 
            {
                list = db.Project.ToList();
            }
            else
            {
                list = user.Projects.ToList();
            }
            return View(list);
        }
    }
}