using Microsoft.AspNetCore.Mvc;

namespace Taskify.Authorization;

/// <summary>
/// Exige autenticación y membresía del equipo dueño del recurso (equipo, proyecto o tarea).
/// </summary>
public sealed class TeamAuthorizeAttribute : TypeFilterAttribute
{
    public TeamAuthorizeAttribute(
        string idRouteKey = "id",
        TeamAuthorizeResource resource = TeamAuthorizeResource.Team,
        TeamAccessLevel minLevel = TeamAccessLevel.Member)
        : base(typeof(TeamAuthorizeFilter))
    {
        Arguments = [idRouteKey, resource, minLevel];
    }
}
