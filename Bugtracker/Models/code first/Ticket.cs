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
        }
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public ApplicationUser AuthorId { get; set; }
        public ApplicationUser AssignedUserId { get; set; }


        public virtual ICollection<ApplicationUser> TicketUser { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<TicketComments> Comments { get; set; }
        public virtual ICollection<Attachments> Attachments { get; set; }
        public virtual ICollection<TicketType> TicketType { get; set; }
        public virtual ICollection<TicketHistory> TicketHistory { get; set; }
        public virtual ICollection<TicketStatus> Status { get; set; }
        public virtual ICollection<TicketPriority> Priority { get; set; }
    }
}