namespace BugTracker.RestServices.Models.OutputModels
{
    public class CommentGetAllOutputModel : CommentOutputModel
    {
        public int BugId { get; set; }

        public string BugTitle { get; set; }
    }
}