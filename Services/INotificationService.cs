using Taskify.Models.Entities;

namespace Taskify.Services;

public interface INotificationService
{
    Task NotifyTaskAssignedAsync(TaskItem task, Guid actorProfileId, CancellationToken cancellationToken = default);

    Task<EmailDispatchResult> NotifyTaskCompletedAsync(TaskItem task, CancellationToken cancellationToken = default);

    Task NotifyProjectCompletedAsync(Project project, CancellationToken cancellationToken = default);

    Task NotifyTaskDueSoonAsync(TaskItem task, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Notification>> ListForUserAsync(Guid profileId, int take, CancellationToken cancellationToken = default);

    Task<int> CountUnreadAsync(Guid profileId, CancellationToken cancellationToken = default);

    Task MarkReadAsync(Guid notificationId, Guid profileId, CancellationToken cancellationToken = default);

    Task MarkAllReadAsync(Guid profileId, CancellationToken cancellationToken = default);
}
