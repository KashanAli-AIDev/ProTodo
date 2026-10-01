using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProTodo.Api.Entities;

namespace ProTodo.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQL Server returns DateTime with Kind=Unspecified; mark as UTC so JSON output ends in "Z".
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(FieldLimits.ProjectName);
        });

        modelBuilder.Entity<TaskItem>(e =>
        {
            e.Property(t => t.Title).IsRequired().HasMaxLength(FieldLimits.TaskTitle);
            e.Property(t => t.Description).HasMaxLength(FieldLimits.TaskDescription);
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

public class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    v => v,
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
