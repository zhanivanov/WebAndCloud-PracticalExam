namespace BugTracker.RestServices.Models.OutputModels
{
    using System.Collections.Generic;

    public class BugDetailedOutputModel : BugOutputModel
    {
        public string Description { get; set; }

        public IEnumerable<CommentOutputModel> Comments { get; set; }
    }
}