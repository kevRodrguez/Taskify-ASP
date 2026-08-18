using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Taskify.Services;

namespace Taskify.Hubs;

[Authorize]
public sealed class TaskBoardHub : Hub
{
    public const string TaskUpdatedEvent = "TaskUpdated";

    private readonly ICurrentUserService _currentUser;
    private readonly ITeamAccessService _teamAccess;

    public TaskBoardHub(ICurrentUserService currentUser, ITeamAccessService teamAccess)
    {
        _currentUser = currentUser;
        _teamAccess = teamAccess;
    }

    public static string GroupName(Guid projectId) => $"project:{projectId:D}";

    public async Task JoinProject(Guid projectId)
    {
        var profileId = await _currentUser.GetProfileIdAsync();
        if (profileId is null || !await _teamAccess.CanAccessProjectAsync(projectId, profileId.Value))
        {
            throw new HubException("No tienes acceso a este proyecto.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(projectId));
    }

    public Task LeaveProject(Guid projectId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(projectId));
    }
}
