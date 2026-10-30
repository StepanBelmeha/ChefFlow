using Microsoft.EntityFrameworkCore;
using TaskService.Models;

namespace TaskService.Data
{
    public class TaskDbContext(DbContextOptions<TaskDbContext> options) : DbContext(options)
    {
        public DbSet<UserTask> Tasks => Set<UserTask>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserTask>()
                .Property(t => t.Title)
                .HasMaxLength(200);

            modelBuilder.Entity<UserTask>()
                .Property(t => t.Priority)
                .HasMaxLength(50);

            modelBuilder.Entity<UserTask>()
                .HasIndex(t => t.UserId);

            modelBuilder.Entity<UserTask>()
                .Property(t => t.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        }
    }
}