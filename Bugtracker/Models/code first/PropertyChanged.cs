using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Bugtracker.Models
{
    public class PropertyChanged
    {
        [Key]
        [Column(Order = 0)]
        public int TicketHistoryId { get; set; }

        [Key]
        [Column(Order = 1)]
        public string Property { get; set; }
    }
}