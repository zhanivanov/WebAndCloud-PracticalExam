namespace BugTracker.RestServices.Models.InputModels
{
    using System.ComponentModel.DataAnnotations;

    public class CommentInputModel
    {
        [Required]
        public string Text { get; set; }
    }
}