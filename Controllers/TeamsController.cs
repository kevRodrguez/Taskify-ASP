using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskify.Authorization;
using Taskify.Models.Enums;
using Taskify.Services;
using Taskify.ViewModels.Teams;

namespace Taskify.Controllers;

[Authorize]
public class TeamsController : Controller
{
    private readonly ICurrentUserService _currentUser;
    private readonly ITeamService _teams;

    public TeamsController(ICurrentUserService currentUser, ITeamService teams)
    {
        _currentUser = currentUser;
        _teams = teams;
    }

    public async Task<IActionResult> Index()
    {
        var profileId = await RequireProfileIdAsync();
        if (profileId is null)
        {
            return Forbid();
        }

        var items = await _teams.ListForUserAsync(profileId.Value);
        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new TeamFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TeamFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var profileId = await RequireProfileIdAsync();
        if (profileId is null)
        {
            return Forbid();
        }

        var team = await _teams.CreateAsync(profileId.Value, model.Name, model.Description);
        TempData["StatusMessage"] = "Equipo creado.";
        return RedirectToAction(nameof(Details), new { id = team.TeamId });
    }

    [HttpGet]
    [TeamAuthorize]
    public async Task<IActionResult> Details(Guid id)
    {
        var team = await _teams.GetAsync(id);
        if (team is null)
        {
            return NotFound();
        }

        var membership = GetMembership();
        var members = await _teams.ListMembersAsync(id);

        return View(new TeamDetailsViewModel
        {
            TeamId = team.TeamId,
            Name = team.Name,
            Description = team.Description,
            CurrentUserRole = membership.Role,
            CanManage = TeamAccess.IsManager(membership.Role),
            IsOwner = membership.Role == TeamMemberRole.Owner,
            Members = members,
            AddMember = new AddTeamMemberViewModel { TeamId = id, Role = TeamMemberRole.Member }
        });
    }

    [HttpGet]
    [TeamAuthorize(minLevel: TeamAccessLevel.Manager)]
    public async Task<IActionResult> Edit(Guid id)
    {
        var team = await _teams.GetAsync(id);
        if (team is null)
        {
            return NotFound();
        }

        return View(new TeamFormViewModel
        {
            TeamId = team.TeamId,
            Name = team.Name,
            Description = team.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [TeamAuthorize(minLevel: TeamAccessLevel.Manager)]
    public async Task<IActionResult> Edit(Guid id, TeamFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.TeamId = id;
            return View(model);
        }

        if (!await _teams.UpdateAsync(id, model.Name, model.Description))
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Equipo actualizado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [TeamAuthorize(minLevel: TeamAccessLevel.Owner)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await _teams.SoftDeleteAsync(id))
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Equipo eliminado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [TeamAuthorize("teamId", minLevel: TeamAccessLevel.Manager)]
    public async Task<IActionResult> AddMember(Guid teamId, AddTeamMemberViewModel model)
    {
        model.TeamId = teamId;
        if (!ModelState.IsValid)
        {
            TempData["StatusMessage"] = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage
                                        ?? "Revisa los datos del miembro.";
            return RedirectToAction(nameof(Details), new { id = teamId });
        }

        var membership = GetMembership();
        if (membership.Role != TeamMemberRole.Owner && model.Role == TeamMemberRole.Owner)
        {
            TempData["StatusMessage"] = "Solo un propietario puede asignar ese rol.";
            return RedirectToAction(nameof(Details), new { id = teamId });
        }

        var result = await _teams.AddMemberByEmailAsync(teamId, model.Email, model.Role);
        TempData["StatusMessage"] = result.Succeeded ? "Miembro agregado." : result.Error;
        return RedirectToAction(nameof(Details), new { id = teamId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [TeamAuthorize("teamId", minLevel: TeamAccessLevel.Manager)]
    public async Task<IActionResult> ChangeRole(Guid teamId, ChangeMemberRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Details), new { id = teamId });
        }

        var membership = GetMembership();
        if (membership.Role != TeamMemberRole.Owner && model.Role == TeamMemberRole.Owner)
        {
            TempData["StatusMessage"] = "Solo un propietario puede otorgar ese rol.";
            return RedirectToAction(nameof(Details), new { id = teamId });
        }

        var result = await _teams.ChangeRoleAsync(teamId, model.ProfileId, model.Role);
        TempData["StatusMessage"] = result.Succeeded ? "Rol actualizado." : result.Error;
        return RedirectToAction(nameof(Details), new { id = teamId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [TeamAuthorize("teamId", minLevel: TeamAccessLevel.Manager)]
    public async Task<IActionResult> RemoveMember(Guid teamId, Guid profileId)
    {
        var result = await _teams.RemoveMemberAsync(teamId, profileId);
        TempData["StatusMessage"] = result.Succeeded ? "Miembro eliminado." : result.Error;
        return RedirectToAction(nameof(Details), new { id = teamId });
    }

    private async Task<Guid?> RequireProfileIdAsync() => await _currentUser.GetProfileIdAsync();

    private Models.Entities.TeamMember GetMembership() =>
        (Models.Entities.TeamMember)HttpContext.Items[TeamAuthorizeFilter.MembershipItemKey]!;
}
