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
    }
    public class UserViewModel
    {
        public ApplicationUser user { get; set; }
        public List<string> roles { get; set; }
    }
}