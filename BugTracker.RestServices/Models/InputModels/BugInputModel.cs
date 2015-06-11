namespace BugTracker.RestServices.Models.InputModels
{
    using System.ComponentModel.DataAnnotations;

    public class BugInputModel
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }
    }
}