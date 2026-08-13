using Microsoft.EntityFrameworkCore;
using Taskify.Data;
using Taskify.Models.Entities;
using Taskify.Models.Enums;

namespace Taskify.Services;

public sealed class TeamAccessService : ITeamAccessService
{
    private readonly TaskifyDbContext _db;

    public TeamAccessService(TaskifyDbContext db)
    {
        _db = db;
    }

    public Task<TeamMember?> GetMembershipAsync(Guid teamId, Guid profileId, CancellationToken cancellationToken = default)
    {
        return _db.TeamMembers
            .AsNoTracking()
            .Include(m => m.Team)
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.ProfileId == profileId, cancellationToken);
    }

    public Task<TeamMember?> GetMembershipByProjectAsync(Guid projectId, Guid profileId, CancellationToken cancellationToken = default)
    {
        return _db.Projects
            .AsNoTracking()
            .Where(p => p.ProjectId == projectId)
            .Join(
                _db.TeamMembers.Include(m => m.Team),
                p => p.TeamId,
                m => m.TeamId,
                (_, m) => m)
            .FirstOrDefaultAsync(m => m.ProfileId == profileId, cancellationToken);
    }

    public Task<TeamMember?> GetMembershipByTaskAsync(Guid taskItemId, Guid profileId, CancellationToken cancellationToken = default)
    {
        return _db.TaskItems
            .AsNoTracking()
            .Where(t => t.TaskItemId == taskItemId)
            .Join(
                _db.Projects,
                t => t.ProjectId,
                p => p.ProjectId,
                (_, p) => p)
            .Join(
                _db.TeamMembers.Include(m => m.Team),
                p => p.TeamId,
                m => m.TeamId,
                (_, m) => m)
            .FirstOrDefaultAsync(m => m.ProfileId == profileId, cancellationToken);
    }

    public async Task<bool> CanAccessTeamAsync(Guid teamId, Guid profileId, CancellationToken cancellationToken = default)
    {
        return await GetMembershipAsync(teamId, profileId, cancellationToken) is not null;
    }

    public async Task<bool> CanManageTeamAsync(Guid teamId, Guid profileId, CancellationToken cancellationToken = default)
    {
        var membership = await GetMembershipAsync(teamId, profileId, cancellationToken);
        return membership is not null && TeamAccess.IsManager(membership.Role);
    }

    public async Task<bool> IsOwnerAsync(Guid teamId, Guid profileId, CancellationToken cancellationToken = default)
    {
        var membership = await GetMembershipAsync(teamId, profileId, cancellationToken);
        return membership?.Role == TeamMemberRole.Owner;
    }

    public async Task<bool> CanAccessProjectAsync(Guid projectId, Guid profileId, CancellationToken cancellationToken = default)
    {
        return await GetMembershipByProjectAsync(projectId, profileId, cancellationToken) is not null;
    }

    public async Task<bool> CanManageProjectAsync(Guid projectId, Guid profileId, CancellationToken cancellationToken = default)
    {
        var membership = await GetMembershipByProjectAsync(projectId, profileId, cancellationToken);
        return membership is not null && TeamAccess.IsManager(membership.Role);
    }

    public IQueryable<Team> TeamsFor(Guid profileId)
    {
        return _db.Teams
            .Where(t => t.Members.Any(m => m.ProfileId == profileId));
    }

    public IQueryable<Project> ProjectsFor(Guid profileId)
    {
        return _db.Projects
            .Where(p => p.Team.Members.Any(m => m.ProfileId == profileId));
    }
}
