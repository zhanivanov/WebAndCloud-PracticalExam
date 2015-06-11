namespace BugTracker.RestServices.Models.OutputModels
{
    using System;

    public class BugOutputModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Status { get; set; }

        public string Author { get; set; }

        public DateTime DateCreated { get; set; }
    }
}