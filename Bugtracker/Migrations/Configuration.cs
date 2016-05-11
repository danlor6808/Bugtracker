namespace Bugtracker.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Bugtracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (!context.Roles.Any(r => r.Name == "Administrator"))
            {
                roleManager.Create(new IdentityRole { Name = "Administrator" });
            }

            if (!context.Roles.Any(r => r.Name == "Project Manager"))
            {
                roleManager.Create(new IdentityRole { Name = "Project Manager" });
            }

            if (!context.Roles.Any(r => r.Name == "Developer"))
            {
                roleManager.Create(new IdentityRole { Name = "Developer" });
            }

            if (!context.Roles.Any(r => r.Name == "Submitter"))
            {
                roleManager.Create(new IdentityRole { Name = "Submitter" });
            }

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            //SuperUser Account
            if (!context.Users.Any(u => u.Email == "danlor6808@gmail.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "danlor6808@gmail.com",
                    Email = "danlor6808@gmail.com",
                    FirstName = "Danny",
                    LastName = "Lorn"
                }, "Password1");
            }

            var userId_SuperUser = userManager.FindByEmail("danlor6808@gmail.com").Id;
            userManager.AddToRole(userId_SuperUser, "Administrator");
            userManager.AddToRole(userId_SuperUser, "Project Manager");
            userManager.AddToRole(userId_SuperUser, "Developer");
            userManager.AddToRole(userId_SuperUser, "Submitter");

            //SuperUser Account
            if (!context.Users.Any(u => u.Email == "guest@123.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "guest@123.com",
                    Email = "guest@123.com",
                    FirstName = "Guest",
                    LastName = "User"
                }, "guest@123.com");
            }

            var userId_Guest = userManager.FindByEmail("guest@123.com").Id;
            userManager.AddToRole(userId_Guest, "Administrator");
            userManager.AddToRole(userId_Guest, "Project Manager");
            userManager.AddToRole(userId_Guest, "Developer");
            userManager.AddToRole(userId_Guest, "Submitter");

            //Guest Developer Account
            if (!context.Users.Any(u => u.Email == "developer@test.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "developer@test.com",
                    Email = "developer@test.com",
                    DisplayName = "Developer",
                    FirstName = "guest",
                    LastName = "account",
                    ProfileIcon = "/upload/profileicon/default.png"
                }, "Password1");
            }

            var userId_developer = userManager.FindByEmail("developer@test.com").Id;
            userManager.AddToRole(userId_developer, "Developer");

            //Guest Submitter Account
            if (!context.Users.Any(u => u.Email == "submitter@test.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "submitter@test.com",
                    Email = "submitter@test.com",
                    DisplayName = "Submitter",
                    FirstName = "guest",
                    LastName = "account",
                    ProfileIcon = "/upload/profileicon/default.png"
                }, "Password1");
            }

            var userId_submitter = userManager.FindByEmail("submitter@test.com").Id;
            userManager.AddToRole(userId_submitter, "Submitter");

        }
    }
}
