using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bugtracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public DateTimeOffset Created { get; set; }
        public string PropertyChanged { get; set; }

        public ApplicationUser userId { get; set; }
    }
}