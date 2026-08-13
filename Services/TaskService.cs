using Microsoft.EntityFrameworkCore;
using Taskify.Data;
using Taskify.Models.Entities;
using Taskify.Models.Enums;
using Taskify.ViewModels.Tasks;

namespace Taskify.Services;

public sealed class TaskService : ITaskService
{
    private readonly TaskifyDbContext _db;

    public TaskService(TaskifyDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TaskCardViewModel>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _db.TaskItems
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.Status)
            .ThenBy(t => t.SortOrder)
            .ThenBy(t => t.CreatedAt)
            .Select(t => new TaskCardViewModel
            {
                TaskItemId = t.TaskItemId,
                ProjectId = t.ProjectId,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                SortOrder = t.SortOrder,
                DueDate = t.DueDate,
                AssignedToProfileId = t.AssignedToProfileId,
                AssignedToName = t.AssignedTo != null ? t.AssignedTo.FullName : null
            })
            .ToListAsync(cancellationToken);
    }

    public Task<TaskItem?> GetAsync(Guid taskItemId, CancellationToken cancellationToken = default)
    {
        return _db.TaskItems
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.TaskItemId == taskItemId, cancellationToken);
    }

    public async Task<TaskItem> CreateAsync(Guid projectId, TaskFormViewModel model, CancellationToken cancellationToken = default)
    {
        var maxOrder = await _db.TaskItems
            .Where(t => t.ProjectId == projectId && t.Status == model.Status)
            .Select(t => (int?)t.SortOrder)
            .MaxAsync(cancellationToken) ?? -1;

        var task = new TaskItem
        {
            ProjectId = projectId,
            Title = model.Title.Trim(),
            Description = TrimToNull(model.Description),
            Status = model.Status,
            SortOrder = maxOrder + 1,
            DueDate = model.DueDate,
            AssignedToProfileId = model.AssignedToProfileId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.TaskItems.Add(task);
        await _db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<bool> UpdateAsync(Guid taskItemId, TaskFormViewModel model, CancellationToken cancellationToken = default)
    {
        var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.TaskItemId == taskItemId, cancellationToken);
        if (task is null)
        {
            return false;
        }

        task.Title = model.Title.Trim();
        task.Description = TrimToNull(model.Description);
        task.DueDate = model.DueDate;
        task.AssignedToProfileId = model.AssignedToProfileId;
        task.Status = model.Status;
        task.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SoftDeleteAsync(Guid taskItemId, CancellationToken cancellationToken = default)
    {
        var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.TaskItemId == taskItemId, cancellationToken);
        if (task is null)
        {
            return false;
        }

        task.DeletedAt = DateTimeOffset.UtcNow;
        task.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<TaskItem?> UpdateStatusAsync(
        Guid taskItemId,
        TaskItemStatus status,
        int sortOrder,
        CancellationToken cancellationToken = default)
    {
        var task = await _db.TaskItems
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.TaskItemId == taskItemId, cancellationToken);
        if (task is null)
        {
            return null;
        }

        task.Status = status;
        task.SortOrder = sortOrder;
        task.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public Task<bool> IsTeamMemberAssigneeAsync(Guid projectId, Guid profileId, CancellationToken cancellationToken = default)
    {
        return _db.Projects
            .Where(p => p.ProjectId == projectId)
            .SelectMany(p => p.Team.Members)
            .AnyAsync(m => m.ProfileId == profileId, cancellationToken);
    }

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
