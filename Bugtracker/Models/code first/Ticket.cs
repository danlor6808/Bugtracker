using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bugtracker.Models
{
    public class Ticket
    {
        public Ticket()
        {
            this.Comments = new HashSet<TicketComments>();
            this.Attachments = new HashSet<Attachments>();
            this.TicketHistory = new HashSet<TicketHistory>();
        }
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public int StatusId { get; set; }
        public int TicketTypeId { get; set; }
        public int PriorityId { get; set; }
        public string AuthorId { get; set; }
        public string AssignedUserId { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public virtual ApplicationUser Author { get; set; }
        public virtual ApplicationUser AssignedUser { get; set; }
        public virtual Project Project { get; set; }

        //public virtual ICollection<ApplicationUser> TicketUser { get; set; }

        public virtual ICollection<TicketComments> Comments { get; set; }
        public virtual ICollection<Attachments> Attachments { get; set; }
        public virtual TicketType TicketType { get; set; }
        public virtual ICollection<TicketHistory> TicketHistory { get; set; }
        public virtual TicketStatus Status { get; set; }
        public virtual TicketPriority Priority { get; set; }
    }
}