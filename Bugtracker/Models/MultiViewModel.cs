using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bugtracker.Models
{
    public class AdminViewModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public MultiSelectList Roles { get; set; }
        public string[] SelectedRoles { get; set; }
        public MultiSelectList Projects { get; set; }
        public int[] CurrentProjects { get; set; }
    }
    public class UserViewModel
    {
        public ApplicationUser user { get; set; }
        public List<string> roles { get; set; }
    }

    public class ProjUserViewModel
    {
        public Project project { get; set; }
        public MultiSelectList Users { get; set; }
        public string[] SelectedUsers { get; set; }
    }

    public class ListViewModel
    {

    }

}