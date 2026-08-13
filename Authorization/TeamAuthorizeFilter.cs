using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Taskify.Models.Entities;
using Taskify.Models.Enums;
using Taskify.Services;

namespace Taskify.Authorization;

public sealed class TeamAuthorizeFilter : IAsyncActionFilter
{
    public const string MembershipItemKey = "Taskify.TeamMembership";

    private readonly string _idRouteKey;
    private readonly TeamAuthorizeResource _resource;
    private readonly TeamAccessLevel _minLevel;
    private readonly ICurrentUserService _currentUser;
    private readonly ITeamAccessService _teamAccess;

    public TeamAuthorizeFilter(
        string idRouteKey,
        TeamAuthorizeResource resource,
        TeamAccessLevel minLevel,
        ICurrentUserService currentUser,
        ITeamAccessService teamAccess)
    {
        _idRouteKey = idRouteKey;
        _resource = resource;
        _minLevel = minLevel;
        _currentUser = currentUser;
        _teamAccess = teamAccess;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        var profile = await _currentUser.GetProfileAsync();
        if (profile is null)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (!TryResolveId(context, out var resourceId))
        {
            context.Result = new NotFoundResult();
            return;
        }

        var membership = await ResolveMembershipAsync(resourceId, profile.ProfileId);
        if (membership is null)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (!MeetsLevel(membership.Role, _minLevel))
        {
            context.Result = new ForbidResult();
            return;
        }

        context.HttpContext.Items[MembershipItemKey] = membership;
        await next();
    }

    private bool TryResolveId(ActionExecutingContext context, out Guid id)
    {
        if (context.ActionArguments.TryGetValue(_idRouteKey, out var argument) && argument is Guid guid)
        {
            id = guid;
            return true;
        }

        var routeValue = context.RouteData.Values[_idRouteKey]?.ToString();
        if (Guid.TryParse(routeValue, out id))
        {
            return true;
        }

        var queryValue = context.HttpContext.Request.Query[_idRouteKey].FirstOrDefault();
        if (Guid.TryParse(queryValue, out id))
        {
            return true;
        }

        if (context.HttpContext.Request.HasFormContentType)
        {
            var formValue = context.HttpContext.Request.Form[_idRouteKey].FirstOrDefault();
            if (Guid.TryParse(formValue, out id))
            {
                return true;
            }
        }

        return false;
    }

    private Task<TeamMember?> ResolveMembershipAsync(Guid resourceId, Guid profileId)
    {
        return _resource switch
        {
            TeamAuthorizeResource.Project => _teamAccess.GetMembershipByProjectAsync(resourceId, profileId),
            TeamAuthorizeResource.Task => _teamAccess.GetMembershipByTaskAsync(resourceId, profileId),
            _ => _teamAccess.GetMembershipAsync(resourceId, profileId)
        };
    }

    private static bool MeetsLevel(TeamMemberRole role, TeamAccessLevel minLevel)
    {
        return minLevel switch
        {
            TeamAccessLevel.Owner => role == TeamMemberRole.Owner,
            TeamAccessLevel.Manager => TeamAccess.IsManager(role),
            _ => true
        };
    }
}
