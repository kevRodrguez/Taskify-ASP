using Microsoft.EntityFrameworkCore;
using Taskify.Models.Entities;

namespace Taskify.Data;

public class TaskifyDbContext : DbContext
{
    public TaskifyDbContext(DbContextOptions<TaskifyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Profile> Profiles => Set<Profile>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskifyDbContext).Assembly);
    }
}
