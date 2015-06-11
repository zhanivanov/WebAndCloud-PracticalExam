using System;
using System.Collections.Generic;
using BugTracker.Data.UnitOfWork;

namespace BugTracker.RestServices.Controllers
{
    using System.Linq;
    using System.Web.Http;
    using Microsoft.AspNet.Identity;

    using BugTracker.Data;
    using BugTracker.RestServices.Models.OutputModels;
    using BugTracker.Data.Models;
    using BugTracker.RestServices.Models.InputModels;

    public class BugsController : ApiController
    {
        private readonly IBugTrackerData db;

        public BugsController()
            : this(new BugTrackerData())
        {
        }

        public BugsController(IBugTrackerData data)
        {
            this.db = data;
        }

        [Route("api/bugs")]
        [HttpGet]
        public IHttpActionResult GetAll()
        {
            var bugs = this.db.Bugs
                .All()
                .OrderByDescending(b => b.DateCreated)
                .Select(b => new BugOutputModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    Status = b.Status.ToString(),
                    Author = b.Author.UserName == null ? null : b.Author.UserName,
                    DateCreated = b.DateCreated
                });

            return Ok(bugs);
        }

        [Route("api/bugs/filter")]
        [HttpGet]
        public IHttpActionResult GetAllWithFilter(
            [FromUri] string keyword = null,
            [FromUri] string statuses = null,
            [FromUri] string author = null)
        {
            IQueryable<Bug> bugsQuery = this.db.Bugs.All();

            if (keyword != null)
            {
                bugsQuery = bugsQuery.Where(b => b.Title.Contains(keyword));
            }

            if (author != null)
            {
                bugsQuery = bugsQuery.Where(b => b.Author.UserName == author);
            }

            var filteredBugs = new List<Bug>();
            if (statuses != null)
            {
                var splittedStatuses = statuses.Split('|');
                foreach (string stringStatus in splittedStatuses)
                {
                    BugStatus bugStatus;
                    Enum.TryParse(stringStatus, out bugStatus);
                    var status = bugsQuery.Where(b => b.Status == bugStatus);
                    filteredBugs.AddRange(status);
                }
            }

            var orderedBugs = filteredBugs
             .OrderByDescending(b => b.DateCreated)
             .Select(b => new BugOutputModel()
             {
                 Id = b.Id,
                 Title = b.Title,
                 Status = b.Status.ToString(),
                 Author = b.Author == null ? null : b.Author.UserName,
                 DateCreated = b.DateCreated

             }).ToList();

            return Ok(orderedBugs);
        }

        [Route("api/bugs/{id}")]
        [HttpGet]
        public IHttpActionResult GetBugById(int id)
        {
            var bug = this.db.Bugs
                .All()
                .Select(b => new BugDetailedOutputModel()
                {
                    Id = b.Id,
                    Description = b.Description,
                    Author = b.Author.UserName == null ? null : b.Author.UserName,
                    DateCreated = b.DateCreated,
                    Status = b.Status.ToString(),
                    Title = b.Title,
                    Comments = b.Comments
                    .OrderByDescending(c => c.PublishDate)
                    .ThenByDescending(c => c.Id)
                    .Select(c => new CommentOutputModel()
                    {
                        Id = c.Id,
                        Text = c.Text,
                        DateCreated = c.PublishDate,
                        Author = c.Author.UserName == null ? null : c.Author.UserName
                    })
                })
                .FirstOrDefault(b => b.Id == id);

            if (bug == null)
            {
                return this.NotFound();
            }

            return Ok(bug);
        }

        [Route("api/bugs")]
        [HttpPost]
        public IHttpActionResult PostNewBug(BugInputModel bugData)
        {
            if (bugData == null)
            {
                return BadRequest("Missing bug data.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = User.Identity.GetUserId();
            var currentUser = this.db.Users.Find(currentUserId);

            var newBug = new Bug()
            {
                Title = bugData.Title,
                Description = bugData.Description,
                AuthorId = currentUserId
            };

            this.db.Bugs.Add(newBug);
            this.db.SaveChanges();

            if (currentUser == null)
            {
                return this.CreatedAtRoute(
                "DefaultApi",
                new { controller = "bugs", id = newBug.Id },
                new { Id = newBug.Id, Message = "Anonymous bug submitted" });
            }

            return this.CreatedAtRoute(
                "DefaultApi",
                new { controller = "bugs", id = newBug.Id },
                new { Id = newBug.Id, Author = currentUser.UserName, Message = "User bug submitted" });
        }

        [Route("api/bugs/{id}")]
        [HttpPatch]
        public IHttpActionResult EditExistingBug(int id, BugInputModel bugData)
        {
            var bugToEdit = db.Bugs.Find(id);
            if (bugToEdit == null)
            {
                return this.NotFound();
            }

            if (!string.IsNullOrEmpty(bugData.Title) && !bugData.Title.Equals("\"\""))
            {
                bugToEdit.Title = bugData.Title;
            }

            if (bugData.Description != null)
            {
                bugToEdit.Description = bugData.Description;
            }

            if (bugData.Status != null)
            {
                BugStatus bugStatus;
                Enum.TryParse(bugData.Status, out bugStatus);
                bugToEdit.Status = bugStatus;
            }

            this.db.SaveChanges();

            return Ok(new
            {
                Message = "Bug #" + id + " patched"
            });
        }

        [Route("api/bugs/{id}")]
        [HttpDelete]
        public IHttpActionResult DeleteBugById(int id)
        {
            var bugToDelete = db.Bugs.Find(id);
            if (bugToDelete == null)
            {
                return this.NotFound();
            }

            this.db.Bugs.Remove(bugToDelete);
            this.db.SaveChanges();

            return Ok(new
            {
                Message = "Bug #" + id + " deleted."
            });
        }
    }
}
