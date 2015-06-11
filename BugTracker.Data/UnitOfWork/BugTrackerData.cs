namespace BugTracker.Data.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;

    using BugTracker.Data.Models;
    using BugTracker.Data.Repositories;

    public class BugTrackerData : IBugTrackerData
    {
        private readonly DbContext dbContext;

        private readonly IDictionary<Type, object> repositories;

         public BugTrackerData()
            : this(new BugTrackerDbContext())
        {
        }

         public BugTrackerData(DbContext dbContext)
        {
            this.dbContext = dbContext;
            this.repositories = new Dictionary<Type, object>();
        }

        public IRepository<Bug> Bugs
        {
            get { return this.GetRepository<Bug>(); }
        }

        public IRepository<Comment> Comments
        {
            get { return this.GetRepository<Comment>(); }
        }

        public IRepository<User> Users
        {
            get { return this.GetRepository<User>(); }
        }

        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }

        private IRepository<T> GetRepository<T>() where T : class
        {
            if (!this.repositories.ContainsKey(typeof(T)))
            {
                var type = typeof(GenericEfRepository<T>);
                this.repositories.Add(typeof(T),
                    Activator.CreateInstance(type, this.dbContext));
            }

            return (IRepository<T>)this.repositories[typeof(T)];
        }
    }
}
