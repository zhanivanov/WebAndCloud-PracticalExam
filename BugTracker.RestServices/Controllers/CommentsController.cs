using BugTracker.Data.UnitOfWork;

namespace BugTracker.RestServices.Controllers
{
    using System.Linq;
    using System.Web.Http;
    using Microsoft.AspNet.Identity;

    using BugTracker.Data;
    using BugTracker.Data.Models;
    using BugTracker.RestServices.Models.InputModels;
    using BugTracker.RestServices.Models.OutputModels;

    public class CommentsController : ApiController
    {
        private readonly IBugTrackerData db;

        public CommentsController() 
            : this(new BugTrackerData())
        {
        }

        public CommentsController(IBugTrackerData data)
        {
            this.db = data;
        }

        [Route("api/comments")]
        [HttpGet]
        public IHttpActionResult GetAll()
        {
            var comments = this.db.Comments
                .All()
                .OrderByDescending(c => c.PublishDate)
                .Select(c => new CommentGetAllOutputModel()
                {
                    Id = c.Id,
                    Text = c.Text,
                    Author = c.Author.UserName == null ? null : c.Author.UserName,
                    DateCreated = c.PublishDate,
                    BugId = c.BugId,
                    BugTitle = c.Bug.Title
                });

            return Ok(comments);
        }

        [Route("api/bugs/{id}/comments")]
        [HttpGet]
        public IHttpActionResult GetCommentForGivenBug(int id)
        {
            var bug = this.db.Bugs.Find(id);
            if (bug == null)
            {
                return this.NotFound();
            }

            var comments = this.db.Comments
                .All()
                .Where(c => c.BugId == id)
                .OrderByDescending(c => c.PublishDate)
                .Select(c => new CommentOutputModel()
                {
                    Id = c.Id,
                    Text = c.Text,
                    Author = c.Author.UserName == null ? null : c.Author.UserName,
                    DateCreated = c.PublishDate
                });

            return Ok(comments);
        }


        [Route("api/bugs/{id}/comments")]
        [HttpPost]
        public IHttpActionResult PostNewComment(int id, CommentInputModel commentData)
        {
            var bug = this.db.Bugs.Find(id);
            if (bug == null)
            {
                return this.NotFound();
            }

            if (commentData == null)
            {
                return BadRequest("Missing bug data.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = User.Identity.GetUserId();
            var currentUser = this.db.Users.Find(currentUserId);

            var newComment = new Comment()
            {
                Text = commentData.Text,
                AuthorId = currentUserId,
                BugId = bug.Id
            };

            this.db.Comments.Add(newComment);
            this.db.SaveChanges();

            if (currentUser == null)
            {
                return this.Ok(new
                { 
                    Id = newComment.Id, 
                    Message = "Added anonymous comment for bug #" + id 
                });
            }

            return this.Ok(new
            {
                Id = newComment.Id, 
                Author = currentUser.UserName, 
                Message = "User comment added for bug #" + id
            });
        }
    }
}
