using DataAccess.Helpers;
using DataAccess.Models;
using DataAccess.Models.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Linq;

namespace DataAccess
{
    public class DesignContextFactory : IDesignTimeDbContextFactory<DataDbContext>
    {
        public DataDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataDbContext>().UseSqlServer(Helper.GetConnectionString());

            return new DataDbContext(optionsBuilder.Options);
        }
    }

    public class DataDbContext : DbContext
    {
        public DataDbContext() : base() { }
        public DataDbContext(DbContextOptions options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(Helper.GetConnectionString());
            base.OnConfiguring(options);
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is BaseModel entity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedOn = DateTime.Now;
                        entity.StateCode = StateCode.Active;
                    }

                    entity.ModifiedOn = DateTime.Now;
                }
            }

            return base.SaveChanges();
        }

        public DbSet<LogEntry> LogEntry { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UsersFolders> UsersFolders { get; set; }
    }
}
