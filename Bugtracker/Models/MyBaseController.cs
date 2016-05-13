using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bugtracker.Models
{
    public class MyBaseController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                ViewBag.Notifications = user.Notifications.OrderByDescending(u => u.Created).ToList();
                ViewBag.DisplayName = user.DisplayName;
                ViewBag.ProfileIcon = user.ProfileIcon;
                ViewBag.FirstName = user.FirstName;
                ViewBag.LastName = user.LastName;
                base.OnActionExecuting(filterContext);
            }
        }
    }
}