using Taskify.Models.Entities;
using Taskify.Models.Enums;

namespace Taskify.Services;

public interface ITeamAccessService
{
    Task<TeamMember?> GetMembershipAsync(Guid teamId, Guid profileId, CancellationToken cancellationToken = default);

    Task<TeamMember?> GetMembershipByProjectAsync(Guid projectId, Guid profileId, CancellationToken cancellationToken = default);

    Task<TeamMember?> GetMembershipByTaskAsync(Guid taskItemId, Guid profileId, CancellationToken cancellationToken = default);

    Task<bool> CanAccessTeamAsync(Guid teamId, Guid profileId, CancellationToken cancellationToken = default);

    Task<bool> CanManageTeamAsync(Guid teamId, Guid profileId, CancellationToken cancellationToken = default);

    Task<bool> IsOwnerAsync(Guid teamId, Guid profileId, CancellationToken cancellationToken = default);

    Task<bool> CanAccessProjectAsync(Guid projectId, Guid profileId, CancellationToken cancellationToken = default);

    Task<bool> CanManageProjectAsync(Guid projectId, Guid profileId, CancellationToken cancellationToken = default);

    IQueryable<Team> TeamsFor(Guid profileId);

    IQueryable<Project> ProjectsFor(Guid profileId);
}

public static class TeamAccess
{
    public static bool IsManager(TeamMemberRole role) =>
        role is TeamMemberRole.Owner or TeamMemberRole.Admin;
}
