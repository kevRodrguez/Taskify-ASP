using Microsoft.EntityFrameworkCore;
using Taskify.Models.Entities;
using Taskify.Validation;

namespace Taskify.Data;

public class TaskifyDbContext : DbContext
{
    public TaskifyDbContext(DbContextOptions<TaskifyDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuthUser> AuthUsers => Set<AuthUser>();

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

    public override int SaveChanges()
    {
        EnumValidator.ValidateTrackedEntities(ChangeTracker);
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnumValidator.ValidateTrackedEntities(ChangeTracker);
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnumValidator.ValidateTrackedEntities(ChangeTracker);
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EnumValidator.ValidateTrackedEntities(ChangeTracker);
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
