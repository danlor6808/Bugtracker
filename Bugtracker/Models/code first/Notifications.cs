using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bugtracker.Models
{
    public class Notifications
    {
        public int id { get; set; }
        public string UserId { get; set; }
        public int TicketId { get; set; }
        public int TypeId { get; set; }
        public string AuthorProfile { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Ticket Ticket { get; set; }
        public virtual NotificationType Type { get; set; }
    }
}