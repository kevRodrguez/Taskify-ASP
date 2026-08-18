using Taskify.Models.Entities;
using Taskify.Models.Enums;
using Taskify.ViewModels.Teams;

namespace Taskify.Services;

public interface ITeamService
{
    Task<IReadOnlyList<TeamListItemViewModel>> ListForUserAsync(Guid profileId, CancellationToken cancellationToken = default);

    Task<Team?> GetAsync(Guid teamId, CancellationToken cancellationToken = default);

    Task<Team> CreateAsync(Guid profileId, string name, string? description, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Guid teamId, string name, string? description, CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteAsync(Guid teamId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeamMemberListItemViewModel>> ListMembersAsync(Guid teamId, CancellationToken cancellationToken = default);

    Task<TeamMemberResult> AddMemberByEmailAsync(Guid teamId, string email, TeamMemberRole role, CancellationToken cancellationToken = default);

    Task<TeamMemberResult> ChangeRoleAsync(Guid teamId, Guid profileId, TeamMemberRole role, CancellationToken cancellationToken = default);

    Task<TeamMemberResult> RemoveMemberAsync(Guid teamId, Guid profileId, CancellationToken cancellationToken = default);
}

public record TeamMemberResult(bool Succeeded, string? Error = null)
{
    public static TeamMemberResult Success() => new(true);

    public static TeamMemberResult Failure(string error) => new(false, error);
}
