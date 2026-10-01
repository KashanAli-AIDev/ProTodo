using Microsoft.EntityFrameworkCore;
using ProTodo.Api.Entities;

namespace ProTodo.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public const int ProjectNameMaxLength = 100;
    public const int TaskTitleMaxLength = 200;
    public const int TaskDescriptionMaxLength = 2000;

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(ProjectNameMaxLength);
        });

        modelBuilder.Entity<TaskItem>(e =>
        {
            e.Property(t => t.Title).IsRequired().HasMaxLength(TaskTitleMaxLength);
            e.Property(t => t.Description).HasMaxLength(TaskDescriptionMaxLength);
            // Stored as a readable string ("Pending", "InProgress", "Completed").
            e.Property(t => t.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

            e.HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Supports the main query: tasks of a project filtered by status.
            e.HasIndex(t => new { t.ProjectId, t.Status });
        });
    }
}
