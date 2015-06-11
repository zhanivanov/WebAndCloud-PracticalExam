namespace BugTracker.Data.UnitOfWork
{
    using BugTracker.Data.Models;
    using BugTracker.Data.Repositories;

    public interface IBugTrackerData
    {
        IRepository<Bug> Bugs { get; }

        IRepository<Comment> Comments { get; }

        IRepository<User> Users { get; }

        void SaveChanges();
    }
}
