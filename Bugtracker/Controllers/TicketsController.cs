using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Bugtracker.Models;
using Microsoft.AspNet.Identity;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNet.Identity.Owin;

namespace Bugtracker.Controllers
{
    [Authorize]
    [RequireHttps]
    public class TicketsController : MyBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private manager _userManager;
        public manager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<manager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Tickets
        public ActionResult Index(int? id, string view)
        {
            ViewBag.Header = "Tickets";
            ViewBag.layout = "sidebar-mini sidebar-collapse fixed";
            ViewBag.Current = "Tickets";
            var list = new List<Ticket>();
            if (view == "all")
            {
                list = db.Ticket.ToList();
                return View(list);
            }
            if (id == null)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                if (User.IsInRole("Administrator"))
                {
                    list = db.Ticket.ToList();
                    return View(list);
                }
                else if (User.IsInRole("Project Manager"))
                {
                    list = user.Projects.SelectMany(u => u.Tickets).ToList();
                    list.AddRange(db.Ticket.Where(u => u.AssignedUserId == user.Id).ToList());
                    list.AddRange(db.Ticket.Where(u => u.AuthorId == user.Id).ToList());
                    return View(list.Distinct());
                }
                else if (User.IsInRole("Developer"))
                {
                    list = db.Ticket.Where(u => u.AuthorId == user.Id).ToList();
                    list.AddRange(db.Ticket.Where(u => u.AssignedUserId == user.Id).ToList());
                    return View(list.Distinct());
                }
                 else
                {
                    list = db.Ticket.Where(u => u.AuthorId == user.Id).ToList();
                    return View(list);
                }
            }
            var project = db.Project.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            ViewBag.projId = project.Id;
            list = project.Tickets.ToList();
            return View(list);
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            ViewBag.Header = "Details";
            ViewBag.layout = "sidebar-mini sidebar-collapse fixed";
            ViewBag.Current = "Tickets";
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Ticket.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            var user = User.Identity.GetUserId();
            //Checks whether current user is an administrator, a user that was assigned to the ticket, or the ticket author
            if (User.IsInRole("Administrator") || ticket.Project.ProjectManagerId == user || ticket.AssignedUserId == user || ticket.AuthorId == user)
            {
                //Checks if status is not closed
                if (ticket.StatusId != 3 || User.IsInRole("Administrator"))
                {
                    var role = db.Roles.First(u => u.Name == "Developer");
                    //var developers = ticket.Project.User.Where(u => u.Roles.Any(r => r.RoleId == role.Id));
                    var developers = db.Users.Where(u => u.Roles.Any(r => r.RoleId == role.Id));
                    ViewBag.AssignedUser = new SelectList(developers, "Id", "DisplayName", ticket.AssignedUserId);
                    ViewBag.TicketStatus = new SelectList(db.TicketStatus, "Id", "Name", ticket.StatusId);
                    ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type", ticket.TicketTypeId);
                    ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name", ticket.PriorityId);
                    var openProject = db.Project.Where(u => u.Status != "Closed");
                    ViewBag.ProjectList = new SelectList(openProject, "Id", "Name", ticket.ProjectId);
                    return View(ticket);
                }
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        public ActionResult Create(int? id)
        {
            ViewBag.Header = "Create";
            ViewBag.layout = "sidebar-mini sidebar-collapse fixed";
            ViewBag.Current = "Tickets";
            if (id == null)
            {
                Ticket ticket = new Ticket();
                ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type");
                ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name");
                var openProject = db.Project.Where(u => u.Status != "Closed");
                ViewBag.ProjectList = new SelectList(openProject, "Id", "Name");
                ViewBag.Bool = true;
                return View(ticket);
            }
            var project = db.Project.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            var user = User.Identity.GetUserId();
            //Checks whether current user is an administrator, a user that was assigned to the project
            if (User.IsInRole("Administrator") || project.Status != "Closed")
            {
                Ticket ticket = new Ticket();
                //ticket.Project = project;
                ViewBag.Bool = false;
                ticket.ProjectId = project.Id;
                ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type");
                ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name");
                return View(ticket);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ProjectId,Title,Body,TicketTypeId,PriorityId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Created = DateTimeOffset.Now;
                ticket.AuthorId = User.Identity.GetUserId();
                //ticket.Project = db.Project.Find(ticket.ProjectId);
                ticket.StatusId = 1;

                ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type", ticket.TicketTypeId);
                ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name", ticket.PriorityId);
                ViewBag.ProjectList = new SelectList(db.Project, "Id", "Name", ticket.ProjectId);

                if (db.Ticket.Any(p => p.Title == ticket.Title && p.ProjectId == ticket.ProjectId))
                {
                    ModelState.AddModelError("Title", "The title must be unique.");

                    return View(ticket);
                }
                var project = db.Project.Find(ticket.ProjectId);
                project.Updated = DateTimeOffset.Now;
                db.Ticket.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Details", "Tickets", new { id = ticket.Id});
            }
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            ViewBag.Header = "Edit";
            ViewBag.layout = "sidebar-mini sidebar-collapse fixed";
            ViewBag.Current = "Tickets";
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Ticket.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            var user = User.Identity.GetUserId();
            //Checks whether current user is an administrator, a user that was assigned to the ticket, or the ticket author
            if (User.IsInRole("Administrator") || ticket.Project.ProjectManagerId == user || ticket.AssignedUserId == user || ticket.AuthorId == user)
            {
                //Checks if status is not closed
                if (ticket.StatusId != 3 || User.IsInRole("Administrator"))
                {
                    var role = db.Roles.First(u => u.Name == "Developer");
                    //var developers = ticket.Project.User.Where(u => u.Roles.Any(r => r.RoleId == role.Id));
                    var developers = db.Users.Where(u => u.Roles.Any(r => r.RoleId == role.Id));
                    ViewBag.AssignedUser = new SelectList(developers, "Id", "DisplayName", ticket.AssignedUserId);
                    ViewBag.TicketStatus = new SelectList(db.TicketStatus, "Id", "Name", ticket.StatusId);
                    ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type", ticket.TicketTypeId);
                    ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name", ticket.PriorityId);
                    var openProject = db.Project.Where(u => u.Status != "Closed");
                    ViewBag.ProjectList = new SelectList(openProject, "Id", "Name", ticket.ProjectId);
                    return View(ticket);
                }
            }
            return HttpNotFound();
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,ProjectId,Title,Body,StatusId,TicketTypeId,PriorityId,AssignedUserId")] Ticket ticket, string id)
        {
            if (ModelState.IsValid)
            {
                var oldticket = db.Ticket.AsNoTracking().FirstOrDefault(u => u.Id == ticket.Id);

                db.Ticket.Attach(ticket);
                db.Entry(ticket).Property("Body").IsModified = true;
                db.Entry(ticket).Property("Title").IsModified = true;
                db.Entry(ticket).Property("TicketTypeId").IsModified = true;
                db.Entry(ticket).Property("PriorityId").IsModified = true;

                //Getting a list of all developers currently assigned to the project
                var role = db.Roles.First(u => u.Name == "Developer");
                var developers = db.Users.Where(u => u.Roles.Any(r => r.RoleId == role.Id));

                if (User.IsInRole("Developer") || User.IsInRole("Administrator"))
                {
                    db.Entry(ticket).Property("StatusId").IsModified = true;
                }

                if (User.IsInRole("Project Manager"))
                {
                    db.Entry(ticket).Property("AssignedUserId").IsModified = true;
                }

                if (User.IsInRole("Administrator"))
                {
                    db.Entry(ticket).Property("ProjectId").IsModified = true;
                }

                if (String.IsNullOrWhiteSpace(ticket.Title))
                {
                    ModelState.AddModelError("Title", "Please enter a valid title.");
                    ViewBag.AssignedUser = new SelectList(developers, "Id", "DisplayName");
                    ViewBag.TicketStatus = new SelectList(db.TicketStatus, "Id", "Name", ticket.StatusId);
                    ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type", ticket.TicketTypeId);
                    ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name", ticket.PriorityId);
                    var openProject = db.Project.Where(u => u.Status != "Closed");
                    ViewBag.ProjectList = new SelectList(openProject, "Id", "Name", ticket.ProjectId);
                    if (id == "editdetails")
                    {
                        return View("Details", ticket);
                    }
                    return View(ticket);
                }

                var TicketAlreadyExists = db.Ticket.Where(p => p.Id == ticket.Id && p.Title == ticket.Title).Select(p => p.Title);
                if (!TicketAlreadyExists.Any())
                {
                    if (db.Ticket.Any(p => p.Title == ticket.Title && p.ProjectId == ticket.ProjectId))
                    {
                        ModelState.AddModelError("Title", "The title must be unique.");
                        ViewBag.AssignedUser = new SelectList(developers, "Id", "DisplayName");
                        ViewBag.TicketStatus = new SelectList(db.TicketStatus, "Id", "Name", ticket.StatusId);
                        ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type", ticket.TicketTypeId);
                        ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name", ticket.PriorityId);
                        var openProject = db.Project.Where(u => u.Status != "Closed");
                        ViewBag.ProjectList = new SelectList(openProject, "Id", "Name", ticket.ProjectId);
                        if (id == "editdetails")
                        {
                            return View("Details", ticket);
                        }
                        return View(ticket);
                    }
                }

                if (ticket.TicketTypeId != 0)
                {
                    switch (ticket.TicketTypeId)
                    {
                        case 1:
                            ViewBag.TicketType = "General";
                            break;
                        case 2:
                            ViewBag.TicketType = "Software";
                            break;
                        case 3:
                            ViewBag.TicketType = "Hardware";
                            break;
                        case 4:
                            ViewBag.TicketType = "Other";
                            break;
                    }
                }

                if (ticket.PriorityId != 0)
                {
                    switch (ticket.PriorityId)
                    {
                        case 1:
                            ViewBag.Priority = "Low";
                            break;
                        case 2:
                            ViewBag.Priority = "Moderate";
                            break;
                        case 3:
                            ViewBag.Priority = "High";
                            break;
                    }
                }

                if (ticket.StatusId != 0)
                {
                    switch (ticket.StatusId)
                    {
                        case 1:
                            ViewBag.Status = "Open";
                            break;
                        case 2:
                            ViewBag.Status = "Work in Progress";
                            break;
                        case 3:
                            ViewBag.Status = "Complete";
                            break;
                    }
                }

                //OLD vs NEW COMPARISONS
                var user = db.Users.Find(User.Identity.GetUserId());
                var ChangeList = new List<string>();
                bool tAssignment = false;

                if (oldticket?.ProjectId != ticket.ProjectId) { var change = "Project has been changed."; ChangeList.Add(change); };
                if (oldticket?.AssignedUserId != ticket.AssignedUserId)
                {
                    if (oldticket.AssignedUserId != null)
                    {
                        ViewBag.User = db.Users.Find(ticket.AssignedUserId).DisplayName;
                        var change = string.Format("Assigned user has been changed from \"{0}\" to \"{1}\".", oldticket.AssignedUser.DisplayName, ViewBag.User);
                        ChangeList.Add(change);
                        tAssignment = true;
                    }
                    else
                    {
                        ViewBag.User = db.Users.Find(ticket.AssignedUserId).DisplayName;
                        var change = string.Format("{0} has been assigned to ticket.", ViewBag.User);
                        ChangeList.Add(change);
                        tAssignment = true;
                    }
                };
                if (oldticket?.Title != ticket.Title) { var change = string.Format("Title has been changed from \"{0}\" to \"{1}\".", oldticket.Title, ticket.Title); ChangeList.Add(change); };
                if (oldticket?.Body != ticket.Body) { var change = "Description has been changed."; ChangeList.Add(change); }
                if (oldticket?.StatusId != ticket.StatusId) {
                    var change = string.Format("Status has been changed from \"{0}\" to \"{1}\".", oldticket.Status.Name, ViewBag.Status);
                    ChangeList.Add(change);
                    
                        if (user.Id != oldticket.Project.ProjectManagerId)
                        {
                            if (ticket.StatusId == 3)
                            {
                                Notifications notification3 = new Notifications
                                {
                                    TicketId = ticket.Id,
                                    UserId = oldticket.Project.ProjectManagerId,
                                    TypeId = 5,
                                    AuthorProfile = user.ProfileIcon,
                                    Created = DateTimeOffset.Now,
                                    Description = string.Format("Has been resolved")
                                };
                            db.Notification.Add(notification3);
                            }
                        }
                };
                if (oldticket?.TicketTypeId != ticket.TicketTypeId) { var change = string.Format("Type has been changed from \"{0}\" to \"{1}\".", oldticket.TicketType.Type, ViewBag.TicketType); ChangeList.Add(change); };
                if (oldticket?.PriorityId != ticket.PriorityId) { var change = string.Format("Priority has been changed from \"{0}\" to \"{1}\".", oldticket.Priority.Name, ViewBag.Priority); ChangeList.Add(change); };
                //checks if a new person has been assigned to the ticket--if true--sends an email to notify them of the change
                if (tAssignment)
                {
                    //Creating email Body
                    StringBuilder sb = new StringBuilder();
                    var callBackUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, protocol: Request.Url.Scheme);
                    sb.Append("You have been assigned to the ticket: <b>" + ticket.Title + "</b><br><br>");
                    sb.Append("Please click <a href=\"" + callBackUrl + "\">here</a> to view details");
                    //Sending email to Assigned User
                    await UserManager.SendEmailAsync(ticket.AssignedUserId, string.Format("Ticket Assignment: {0}", ticket.Title), sb.ToString());

                    Notifications notification = new Notifications
                    {
                        TicketId = ticket.Id,
                        UserId = ticket.AssignedUserId,
                        TypeId = 1,
                        AuthorProfile = user.ProfileIcon,
                        Created = DateTimeOffset.Now,
                        Description = string.Format("New assignment")
                    };
                    db.Notification.Add(notification);
                }

                //Check if there were any changes made
                if (ChangeList.Count != 0)
                {
                    //Saving Ticket History
                    TicketHistory tHistory = new TicketHistory
                    {
                        TicketId = ticket.Id,
                        Created = DateTimeOffset.Now,
                        UserId = User.Identity.GetUserId(),
                        PropertyChanged = string.Concat(ChangeList.ToArray())
                    };

                    if (ticket.AssignedUserId != null)
                    {
                        //Send email of changes if the person who made the changes IS NOT assigned to the ticket
                        if (User.Identity.GetUserId() != ticket.AssignedUserId)
                        {
                            //Creating email Body
                            StringBuilder sb = new StringBuilder();
                            var callBackUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, protocol: Request.Url.Scheme);
                            sb.Append("Changes have been made to the ticket: <b>" + ticket.Title + "</b><br>");
                            sb.Append("<ul>");
                            foreach (var change in ChangeList)
                            {
                                sb.AppendFormat("<li><em>{0}</em></li>", change);
                            };
                            sb.Append("</ul>");
                            sb.Append("Please click <a href=\"" + callBackUrl + "\">here</a> to view details");
                            //Sending email to Assigned User
                            await UserManager.SendEmailAsync(ticket.AssignedUserId, string.Format("Ticket Updated: {0}", ticket.Title), sb.ToString());

                            Notifications notification2 = new Notifications
                            {
                                TicketId = ticket.Id,
                                UserId = ticket.AssignedUserId,
                                TypeId = 2,
                                AuthorProfile = user.ProfileIcon,
                                Created = DateTimeOffset.Now,
                                Description = string.Format("Has been modified")
                            };
                            db.Notification.Add(notification2);
                        }
                    }

                    db.TicketHistory.Add(tHistory);
                }

                var project = db.Project.Find(ticket.ProjectId);
                project.Updated = DateTimeOffset.Now;
                ticket.Updated = DateTimeOffset.Now;
                db.SaveChanges();
                return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
            }
            if (id == "editdetails")
            {
                return View("Details", ticket);
            }
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            ViewBag.Header = "Delete";
            ViewBag.layout = "sidebar-mini sidebar-collapse fixed";
            ViewBag.Current = "Tickets";
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Ticket.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            var project = db.Project.Find(ticket.ProjectId);
            project.Updated = DateTimeOffset.Now;
            db.Ticket.Remove(ticket);
            db.SaveChanges();
            ViewBag.StatusMessage = "Ticket has been successfully deleted";
            return RedirectToAction("Index", "Tickets");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //Comments Section///////////////////////////////////////////////////

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> CreateComment(TicketComments comment)
        {
            if (ModelState.IsValid)
            {
                var ticket = db.Ticket.Find(comment.TicketId);
                if(User.Identity.GetUserId() == ticket.AuthorId || User.Identity.GetUserId() == ticket.AssignedUserId || User.Identity.GetUserId() == ticket.Project.ProjectManagerId || User.IsInRole("Administrator"))
                    {
                    ticket.Updated = DateTimeOffset.Now;
                    comment.AuthorId = User.Identity.GetUserId();
                    comment.Created = System.DateTimeOffset.Now;
                    var project = db.Project.Find(ticket.ProjectId);
                    project.Updated = DateTimeOffset.Now;
                    db.TicketComment.Add(comment);


                    if (ticket.AssignedUserId != null)
                    {
                        //Send email of changes if the person who made the changes IS NOT assigned to the ticket
                        if (User.Identity.GetUserId() != ticket.AssignedUserId)
                        {
                            //Creating email Body
                            StringBuilder sb = new StringBuilder();
                            var callBackUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, protocol: Request.Url.Scheme);
                            sb.Append("A comment has been made on the ticket: <b>" + ticket.Title + "</b><br><br>");
                            sb.Append(string.Format("<b>{0}</b>: <em>{1}</em><br>", db.Users.Find(User.Identity.GetUserId()).DisplayName, comment.Body));
                            sb.Append("<br>Please click <a href=\"" + callBackUrl + "\">here</a> to view details");
                            //Sending email to Assigned User
                            await UserManager.SendEmailAsync(ticket.AssignedUserId, string.Format("Ticket Commented On: {0}", ticket.Title), sb.ToString());
                            var user = db.Users.Find(User.Identity.GetUserId());

                            Notifications notification = new Notifications
                            {
                                TicketId = ticket.Id,
                                UserId = ticket.AssignedUserId,
                                TypeId = 4,
                                AuthorProfile = user.ProfileIcon,
                                Created = DateTimeOffset.Now,
                                Description = string.Format("New comment")
                            };
                            db.Notification.Add(notification);
                        }
                    }
                    db.SaveChanges();
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            return RedirectToAction("Details", "Tickets", new { id = comment.TicketId });
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteComment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComments comment = db.TicketComment.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            var project = db.Project.Find(comment.Ticket.ProjectId);
            project.Updated = DateTimeOffset.Now;

            db.TicketComment.Remove(comment);
            db.SaveChanges();
            return RedirectToAction("Details", "Tickets", new { id = comment.TicketId });
        }

        public ActionResult EditComment(int? id)
        {
            ViewBag.layout = "sidebar-mini sidebar-collapse fixed";
            ViewBag.Current = "Tickets";
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComments comment = db.TicketComment.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            if (User.Identity.GetUserId() != comment.AuthorId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(comment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditComment(TicketComments comment)
        {
            if (ModelState.IsValid)
            {
                var ticket = db.Ticket.Find(comment.TicketId);
                ticket.Updated = DateTimeOffset.Now;
                comment.Updated = System.DateTimeOffset.Now;
                var project = db.Project.Find(ticket.ProjectId);
                project.Updated = DateTimeOffset.Now;
                db.TicketComment.Attach(comment);
                db.Entry(comment).Property("Body").IsModified = true;
                db.Entry(comment).Property("Updated").IsModified = true;
                db.SaveChanges();
                return RedirectToAction("Details", "Tickets", new { id = comment.TicketId });
            }
            return View(comment);
        }

        //Attachments Section///////////////////////////////////////////////////

        [HttpPost]
        public async Task<ActionResult> Upload(Attachments attachments, int? tId, IEnumerable<HttpPostedFileBase> files)
        {
            var ticket = db.Ticket.Find(tId);
            var projectName = ticket.Project.Name;
            var ticketId = ticket.Id;

            //Checks whether user has been assigned to the ticket, is the author of the ticket, or is in the role of administrator
            if (User.IsInRole("Administrator") || User.Identity.GetUserId() == ticket.AssignedUserId || User.Identity.GetUserId() == ticket.AuthorId || User.Identity.GetUserId() == ticket.Project.ProjectManagerId)
            {
                var fileList = new List<string>();
                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        //Create directory based on project name + ticket id
                        var path = Server.MapPath("/upload/" + projectName + "/" + ticketId + "/");
                        Directory.CreateDirectory(path);

                        //var fileName = string.Format("{0}" + Path.GetExtension(file.FileName), DateTime.Now.ToString("yyyyMMdd"));
                        var num = 0;
                        var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                        var fileurl = Path.Combine("/upload/", projectName, ticketId.ToString(), fileName + Path.GetExtension(file.FileName));

                        //Checks if file url matches any of the current attachments, 
                        //if so it will loop and add a (number) to the end of the filename
                        fileCheck:
                        if (ticket.Attachments.Any(u => u.FileURL == fileurl))
                        {
                            //Sets "filename" back to the default value
                            fileName = Path.GetFileNameWithoutExtension(file.FileName);
                            //Add's parentheses after the name with a number ex. filename(4)
                            fileName = string.Format(fileName + "(" + ++num + ")");
                            //Makes sure the fileurl gets updated with the new filename so it could check
                            fileurl = Path.Combine("/upload/", projectName, ticketId.ToString(), fileName + Path.GetExtension(file.FileName));
                            goto fileCheck;
                        }

                        //save path for the file uploaded
                        string savePath = Server.MapPath("/upload/" + projectName + "/" + ticketId + "/" + fileName + Path.GetExtension(file.FileName));
                        file.SaveAs(savePath);

                        //Attachment details
                        attachments.TicketId = ticket.Id;
                        attachments.Name = string.Format(fileName + Path.GetExtension(file.FileName));
                        attachments.UserId = User.Identity.GetUserId();
                        attachments.FileURL = fileurl;
                        attachments.Uploaded = DateTimeOffset.Now;

                        //Add name of attachment to list for email
                        fileList.Add(attachments.Name);

                        //Save Changes
                        db.Attachment.Add(attachments);
                        db.SaveChanges();
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }

                if (ticket.AssignedUserId != null)
                {
                    //Send email of changes if the person who made the changes IS NOT assigned to the ticket
                    if (User.Identity.GetUserId() != ticket.AssignedUserId)
                    {
                        //Creating email Body
                        StringBuilder sb = new StringBuilder();
                        var callBackUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, protocol: Request.Url.Scheme);
                        sb.Append("Attachment(s) have been added to the ticket: <b>" + ticket.Title + "</b><br>");
                        sb.Append("<ul>");
                        foreach (var file in fileList)
                        {
                            sb.AppendFormat("<li><em>{0}</em></li>", file);
                        };
                        sb.Append("</ul>");
                        sb.Append(string.Format("Uploaded by: {0}", db.Users.Find(User.Identity.GetUserId()).DisplayName));
                        sb.Append("<br><br>Please click <a href=\"" + callBackUrl + "\">here</a> to view details");
                        //Sending email to Assigned User
                        await UserManager.SendEmailAsync(ticket.AssignedUserId, string.Format("Ticket Attachment(s) Added: {0}", ticket.Title), sb.ToString());
                        var user = db.Users.Find(User.Identity.GetUserId());

                        Notifications notification = new Notifications
                        {
                            TicketId = ticket.Id,
                            UserId = ticket.AssignedUserId,
                            TypeId = 3,
                            AuthorProfile = user.ProfileIcon,
                            Created = DateTimeOffset.Now,
                            Description = string.Format("New attachment")
                        };
                        db.Notification.Add(notification);
                    }
                }
                //Ticket Update
                ticket.Updated = DateTimeOffset.Now;
                //Project Update
                var project = db.Project.Find(ticket.ProjectId);
                project.Updated = DateTimeOffset.Now;
                db.SaveChanges();
                return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        public ActionResult DeleteAttachment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var attachment = db.Attachment.Find(id);
            if (attachment == null)
            {
                return HttpNotFound();
            }
            if (User.Identity.GetUserId() == attachment.UserId)
            {
                var ticket = db.Ticket.Where(u => u.Attachments.Any(a => a.Id == id)).FirstOrDefault();
                ticket.Updated = DateTimeOffset.Now;
                db.Attachment.Remove(attachment);
                db.SaveChanges();
                return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        /////////////////////////////////Create Ticket Partial!

        // GET: Tickets/Create
        public ActionResult CreateTicketPartial(int? id)
        {
            ViewBag.Header = "Create";
            ViewBag.layout = "sidebar-mini";
            if (id == null)
            {
                Ticket ticket = new Ticket();
                ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type");
                ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name");
                ViewBag.ProjectList = new SelectList(db.Project, "Id", "Name");
                ViewBag.Bool = true;
                return View(ticket);
            }
            var project = db.Project.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            var user = User.Identity.GetUserId();
            //Checks whether current user is an administrator, a user that was assigned to the project
            if (User.IsInRole("Administrator") || project.Status != "Closed")
            {
                Ticket ticket = new Ticket();
                //ticket.Project = project;
                ViewBag.Bool = false;
                ticket.ProjectId = project.Id;
                ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type");
                ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name");
                ViewBag.ProjectName = project.Name;
                return View(ticket);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTicketPartial([Bind(Include = "Id,ProjectId,Title,Body,TicketTypeId,PriorityId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Created = DateTimeOffset.Now;
                ticket.AuthorId = User.Identity.GetUserId();
                ticket.StatusId = 1;

                ViewBag.TicketType = new SelectList(db.TicketType, "Id", "Type", ticket.TicketTypeId);
                ViewBag.TicketPriority = new SelectList(db.TicketPriority, "Id", "Name", ticket.PriorityId);
                ViewBag.ProjectList = new SelectList(db.Project, "Id", "Name", ticket.ProjectId);

                if (db.Ticket.Any(p => p.Title == ticket.Title && p.ProjectId == ticket.ProjectId))
                {
                    ModelState.AddModelError("Title", "The title must be unique.");

                    return View(ticket);
                }
                db.Ticket.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
            }
            return View(ticket);
        }



    }
}
