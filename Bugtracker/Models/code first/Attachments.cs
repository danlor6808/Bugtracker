using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bugtracker.Models
{
    public class Attachments
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Description { get; set; }
        public string FileURL { get; set; }

        public ApplicationUser userId { get; set; }
    }
}