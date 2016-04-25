using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bugtracker.Models
{
    public class ProjectStatus
    {
        public int Id { get; set; }
        public SelectList Type { get; set; }
    }
}