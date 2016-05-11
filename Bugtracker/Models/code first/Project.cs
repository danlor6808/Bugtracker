using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bugtracker.Models
{
    public class Project
    {
        public Project()
        {
            this.Tickets = new HashSet<Ticket>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public string Status { get; set; }
        public DateTimeOffset? Deadline { get; set; }
        public string ProjectManagerId { get; set; }

        public virtual ApplicationUser ProjectManager { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}