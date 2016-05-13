using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bugtracker.Models
{ 
    public class NotificationType
    {
    public int id { get; set; }
    public string Type { get; set; }
    public ICollection<Notifications> Notifications { get; set; }
    }
}