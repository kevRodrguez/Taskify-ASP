using Microsoft.EntityFrameworkCore;
using Taskify.Data;
using Taskify.Models.Entities;
using Taskify.Models.Enums;
using Taskify.ViewModels.Teams;

namespace Taskify.Services;

public sealed class TeamService : ITeamService
{
    private readonly TaskifyDbContext _db;

    public TeamService(TaskifyDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TeamListItemViewModel>> ListForUserAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        return await _db.Teams
            .Where(t => t.Members.Any(m => m.ProfileId == profileId))
            .OrderBy(t => t.Name)
            .Select(t => new TeamListItemViewModel
            {
                TeamId = t.TeamId,
                Name = t.Name,
                Description = t.Description,
                MemberCount = t.Members.Count,
                ProjectCount = t.Projects.Count(p => p.Status == ProjectStatus.Active),
                Role = t.Members.First(m => m.ProfileId == profileId).Role
            })
            .ToListAsync(cancellationToken);
    }

    public Task<Team?> GetAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return _db.Teams.FirstOrDefaultAsync(t => t.TeamId == teamId, cancellationToken);
    }

    public async Task<Team> CreateAsync(Guid profileId, string name, string? description, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var team = new Team
        {
            Name = name.Trim(),
            Description = TrimToNull(description),
            CreatedBy = profileId,
            CreatedAt = now
        };

        team.Members.Add(new TeamMember
        {
            ProfileId = profileId,
            Role = TeamMemberRole.Owner,
            JoinedAt = now
        });

        _db.Teams.Add(team);
        await _db.SaveChangesAsync(cancellationToken);
        return team;
    }

    public async Task<bool> UpdateAsync(Guid teamId, string name, string? description, CancellationToken cancellationToken = default)
    {
        var team = await _db.Teams.FirstOrDefaultAsync(t => t.TeamId == teamId, cancellationToken);
        if (team is null)
        {
            return false;
        }

        team.Name = name.Trim();
        team.Description = TrimToNull(description);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SoftDeleteAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var team = await _db.Teams.FirstOrDefaultAsync(t => t.TeamId == teamId, cancellationToken);
        if (team is null)
        {
            return false;
        }

        team.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<TeamMemberListItemViewModel>> ListMembersAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _db.TeamMembers
            .Where(m => m.TeamId == teamId)
            .OrderBy(m => m.Role)
            .ThenBy(m => m.Profile.FullName)
            .Select(m => new TeamMemberListItemViewModel
            {
                ProfileId = m.ProfileId,
                FullName = m.Profile.FullName,
                Email = m.Profile.Email,
                Role = m.Role,
                JoinedAt = m.JoinedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TeamMemberResult> AddMemberByEmailAsync(Guid teamId, string email, TeamMemberRole role, CancellationToken cancellationToken = default)
    {
        if (role == TeamMemberRole.Owner)
        {
            return TeamMemberResult.Failure("No puedes agregar otro propietario. Transfiere el rol desde un miembro existente.");
        }

        var normalized = email.Trim().ToLowerInvariant();
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.Email.ToLower() == normalized, cancellationToken);
        if (profile is null)
        {
            return TeamMemberResult.Failure("No hay un usuario registrado con ese correo.");
        }

        var exists = await _db.TeamMembers.AnyAsync(
            m => m.TeamId == teamId && m.ProfileId == profile.ProfileId,
            cancellationToken);
        if (exists)
        {
            return TeamMemberResult.Failure("Esa persona ya forma parte del equipo.");
        }

        _db.TeamMembers.Add(new TeamMember
        {
            TeamId = teamId,
            ProfileId = profile.ProfileId,
            Role = role,
            JoinedAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
        return TeamMemberResult.Success();
    }

    public async Task<TeamMemberResult> ChangeRoleAsync(Guid teamId, Guid profileId, TeamMemberRole role, CancellationToken cancellationToken = default)
    {
        var member = await _db.TeamMembers.FirstOrDefaultAsync(
            m => m.TeamId == teamId && m.ProfileId == profileId,
            cancellationToken);
        if (member is null)
        {
            return TeamMemberResult.Failure("Ese miembro no está en el equipo.");
        }

        if (member.Role == TeamMemberRole.Owner && role != TeamMemberRole.Owner)
        {
            var ownerCount = await _db.TeamMembers.CountAsync(
                m => m.TeamId == teamId && m.Role == TeamMemberRole.Owner,
                cancellationToken);
            if (ownerCount <= 1)
            {
                return TeamMemberResult.Failure("El equipo debe conservar al menos un propietario.");
            }
        }

        member.Role = role;
        await _db.SaveChangesAsync(cancellationToken);
        return TeamMemberResult.Success();
    }

    public async Task<TeamMemberResult> RemoveMemberAsync(Guid teamId, Guid profileId, CancellationToken cancellationToken = default)
    {
        var member = await _db.TeamMembers.FirstOrDefaultAsync(
            m => m.TeamId == teamId && m.ProfileId == profileId,
            cancellationToken);
        if (member is null)
        {
            return TeamMemberResult.Failure("Ese miembro no está en el equipo.");
        }

        if (member.Role == TeamMemberRole.Owner)
        {
            var ownerCount = await _db.TeamMembers.CountAsync(
                m => m.TeamId == teamId && m.Role == TeamMemberRole.Owner,
                cancellationToken);
            if (ownerCount <= 1)
            {
                return TeamMemberResult.Failure("No puedes quitar al único propietario del equipo.");
            }
        }

        _db.TeamMembers.Remove(member);
        await _db.SaveChangesAsync(cancellationToken);
        return TeamMemberResult.Success();
    }

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
