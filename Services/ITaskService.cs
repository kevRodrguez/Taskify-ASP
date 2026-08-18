using Taskify.Models.Entities;
using Taskify.Models.Enums;
using Taskify.ViewModels.Tasks;

namespace Taskify.Services;

public interface ITaskService
{
    Task<IReadOnlyList<TaskCardViewModel>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<TaskItem?> GetAsync(Guid taskItemId, CancellationToken cancellationToken = default);

    Task<TaskItem> CreateAsync(Guid projectId, TaskFormViewModel model, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Guid taskItemId, TaskFormViewModel model, CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteAsync(Guid taskItemId, CancellationToken cancellationToken = default);

    Task<TaskItem?> UpdateStatusAsync(Guid taskItemId, TaskItemStatus status, int sortOrder, CancellationToken cancellationToken = default);

    Task<bool> IsTeamMemberAssigneeAsync(Guid projectId, Guid profileId, CancellationToken cancellationToken = default);
}
