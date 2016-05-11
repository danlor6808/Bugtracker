using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bugtracker.Models
{
    public class ProjectUserHelper
    {
        private ApplicationDbContext db;

        public ProjectUserHelper(ApplicationDbContext context)
        {
            this.db = context;
        }

        //public List<string> currentProjectUsers (int id)
        //{
        //    var project = db.Project.Find(id).User;
        //    var result = new List<string>();
        //    foreach (var user in project)
        //    {
        //        result.Add(user.Id);
        //    }
        //    return result;
        //}

        //public bool deleteAllUsers (int id)
        //{
        //    var project = db.Project.Find(id).User;
        //    foreach (var user in project)
        //    {
        //        project.Remove(user);
        //    }
        //    return true;
        //}

        //public bool removeProjectUser (int projectid, string userid)
        //{
        //    var project = db.Project.Find(projectid);
        //    var user = project.User.First(u => u.Id == userid);
        //    var result = project.User.Remove(user);
        //    return result;
        //}

        //public bool addProjectUser(int projectid, string userid)
        //{
        //    var project = db.Project.Find(projectid);
        //    var user = db.Users.First(u => u.Id == userid);
        //    project.User.Add(user);
        //    return true;
        //}

        public SelectListItem[] statuslist (int projectId)
        {
            var project = db.Project.Find(projectId);
            var list = new[]
            {
            new SelectListItem() { Text = "Open", Value = "Open" },
            new SelectListItem() { Text = "Closed", Value = "Closed" },
            new SelectListItem() { Text = "Work in Progress", Value = "Work in Progress" }
            };

            var SelectedStatus = list.First(d => d.Value == project.Status);
            SelectedStatus.Selected = true;
            return list;
        }
    }
}