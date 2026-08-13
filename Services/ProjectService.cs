using Microsoft.EntityFrameworkCore;
using Taskify.Data;
using Taskify.Models.Entities;
using Taskify.Models.Enums;
using Taskify.ViewModels.Projects;

namespace Taskify.Services;

public sealed class ProjectService : IProjectService
{
    private readonly TaskifyDbContext _db;

    public ProjectService(TaskifyDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProjectListItemViewModel>> ListForUserAsync(
        Guid profileId,
        ProjectStatus? status,
        Guid? teamId,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Projects.Where(p => p.Team.Members.Any(m => m.ProfileId == profileId));

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (teamId.HasValue)
        {
            query = query.Where(p => p.TeamId == teamId.Value);
        }

        return await query
            .OrderBy(p => p.DueDate)
            .ThenBy(p => p.Name)
            .Select(p => new ProjectListItemViewModel
            {
                ProjectId = p.ProjectId,
                TeamId = p.TeamId,
                TeamName = p.Team.Name,
                Name = p.Name,
                Description = p.Description,
                StartDate = p.StartDate,
                DueDate = p.DueDate,
                Status = p.Status,
                TaskCount = p.Tasks.Count()
            })
            .ToListAsync(cancellationToken);
    }

    public Task<Project?> GetAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return _db.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId, cancellationToken);
    }

    public Task<Project?> GetWithTeamAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return _db.Projects
            .Include(p => p.Team)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId, cancellationToken);
    }

    public async Task<Project> CreateAsync(Guid teamId, ProjectFormViewModel model, CancellationToken cancellationToken = default)
    {
        var project = new Project
        {
            TeamId = teamId,
            Name = model.Name.Trim(),
            Description = TrimToNull(model.Description),
            StartDate = model.StartDate,
            DueDate = model.DueDate,
            Status = ProjectStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task<bool> UpdateAsync(Guid projectId, ProjectFormViewModel model, CancellationToken cancellationToken = default)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId, cancellationToken);
        if (project is null)
        {
            return false;
        }

        project.Name = model.Name.Trim();
        project.Description = TrimToNull(model.Description);
        project.StartDate = model.StartDate;
        project.DueDate = model.DueDate;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetStatusAsync(Guid projectId, ProjectStatus status, CancellationToken cancellationToken = default)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId, cancellationToken);
        if (project is null)
        {
            return false;
        }

        project.Status = status;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
