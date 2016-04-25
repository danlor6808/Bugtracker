using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bugtracker.Models
{
    public class TicketComments
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public ApplicationUser AuthorId { get; set; }
    }
}