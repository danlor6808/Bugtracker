using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections;
using System.Collections.Generic;

namespace Bugtracker.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ApplicationUser()
        {
            this.Projects = new HashSet<Project>();
            this.Tickets = new HashSet<Ticket>();
            this.Comments = new HashSet<TicketComments>();
            this.Attachments = new HashSet<Attachments>();
            this.TicketHistory = new HashSet<TicketHistory>();
        }

        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<TicketComments> Comments { get; set; }
        public virtual ICollection<Attachments> Attachments { get; set; }
        public virtual ICollection<TicketHistory> TicketHistory { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Project> Project { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<TicketComments> TicketComment { get; set; }
        public DbSet<Attachments> Attachment { get; set; }
        public DbSet<TicketHistory> TicketHistory { get; set; }
    }
}