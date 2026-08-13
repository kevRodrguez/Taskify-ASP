using Taskify.Models.Entities;
using Taskify.Models.Enums;
using Taskify.ViewModels.Projects;

namespace Taskify.Services;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectListItemViewModel>> ListForUserAsync(
        Guid profileId,
        ProjectStatus? status,
        Guid? teamId,
        CancellationToken cancellationToken = default);

    Task<Project?> GetAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<Project?> GetWithTeamAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<Project> CreateAsync(Guid teamId, ProjectFormViewModel model, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Guid projectId, ProjectFormViewModel model, CancellationToken cancellationToken = default);

    Task<bool> SetStatusAsync(Guid projectId, ProjectStatus status, CancellationToken cancellationToken = default);
}
