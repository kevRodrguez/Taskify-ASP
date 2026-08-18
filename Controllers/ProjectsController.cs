using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Taskify.Authorization;
using Taskify.Models.Enums;
using Taskify.Services;
using Taskify.ViewModels.Projects;

namespace Taskify.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectService _projects;
    private readonly ITeamAccessService _access;
    private readonly INotificationService _notifications;

    public ProjectsController(
        ICurrentUserService currentUser,
        IProjectService projects,
        ITeamAccessService access,
        INotificationService notifications)
    {
        _currentUser = currentUser;
        _projects = projects;
        _access = access;
        _notifications = notifications;
    }

    public async Task<IActionResult> Index(Guid? teamId, ProjectStatus? status)
    {
        var profileId = await _currentUser.GetProfileIdAsync();
        if (profileId is null)
        {
            return Forbid();
        }

        var items = await _projects.ListForUserAsync(profileId.Value, status ?? ProjectStatus.Active, teamId);
        ViewBag.Status = status ?? ProjectStatus.Active;
        ViewBag.TeamId = teamId;
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid? teamId)
    {
        var model = new ProjectFormViewModel();
        if (teamId.HasValue)
        {
            model.TeamId = teamId.Value;
        }

        if (!await PopulateTeamsAsync(model))
        {
            TempData["StatusMessage"] = "Necesitas ser propietario o administrador de un equipo para crear proyectos.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectFormViewModel model)
    {
        var profileId = await _currentUser.GetProfileIdAsync();
        if (profileId is null)
        {
            return Forbid();
        }

        if (!await _access.CanManageTeamAsync(model.TeamId, profileId.Value))
        {
            ModelState.AddModelError(nameof(model.TeamId), "No puedes crear proyectos en ese equipo.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateTeamsAsync(model);
            return View(model);
        }

        var project = await _projects.CreateAsync(model.TeamId, model);
        TempData["StatusMessage"] = "Proyecto creado.";
        return RedirectToAction(nameof(Details), new { id = project.ProjectId });
    }

    [HttpGet]
    [TeamAuthorize(resource: TeamAuthorizeResource.Project)]
    public async Task<IActionResult> Details(Guid id)
    {
        var project = await _projects.GetWithTeamAsync(id);
        if (project is null)
        {
            return NotFound();
        }

        var membership = (Models.Entities.TeamMember)HttpContext.Items[TeamAuthorizeFilter.MembershipItemKey]!;
        return View(new ProjectDetailsViewModel
        {
            ProjectId = project.ProjectId,
            TeamId = project.TeamId,
            TeamName = project.Team.Name,
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            DueDate = project.DueDate,
            Status = project.Status,
            CanManage = TeamAccess.IsManager(membership.Role)
        });
    }

    [HttpGet]
    [TeamAuthorize(resource: TeamAuthorizeResource.Project, minLevel: TeamAccessLevel.Manager)]
    public async Task<IActionResult> Edit(Guid id)
    {
        var project = await _projects.GetWithTeamAsync(id);
        if (project is null)
        {
            return NotFound();
        }

        var model = new ProjectFormViewModel
        {
            ProjectId = project.ProjectId,
            TeamId = project.TeamId,
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            DueDate = project.DueDate
        };
        await PopulateTeamsAsync(model, lockTeam: true);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [TeamAuthorize(resource: TeamAuthorizeResource.Project, minLevel: TeamAccessLevel.Manager)]
    public async Task<IActionResult> Edit(Guid id, ProjectFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ProjectId = id;
            await PopulateTeamsAsync(model, lockTeam: true);
            return View(model);
        }

        if (!await _projects.UpdateAsync(id, model))
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Proyecto actualizado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [TeamAuthorize(resource: TeamAuthorizeResource.Project, minLevel: TeamAccessLevel.Manager)]
    public async Task<IActionResult> Complete(Guid id)
    {
        var project = await _projects.GetAsync(id);
        if (project is null)
        {
            return NotFound();
        }

        if (!await _projects.SetStatusAsync(id, ProjectStatus.Completed))
        {
            return NotFound();
        }

        project.Status = ProjectStatus.Completed;
        await _notifications.NotifyProjectCompletedAsync(project);
        TempData["StatusMessage"] = "Proyecto marcado como finalizado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [TeamAuthorize(resource: TeamAuthorizeResource.Project, minLevel: TeamAccessLevel.Manager)]
    public async Task<IActionResult> Archive(Guid id)
    {
        if (!await _projects.SetStatusAsync(id, ProjectStatus.Archived))
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Proyecto archivado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [TeamAuthorize(resource: TeamAuthorizeResource.Project, minLevel: TeamAccessLevel.Manager)]
    public async Task<IActionResult> Reopen(Guid id)
    {
        if (!await _projects.SetStatusAsync(id, ProjectStatus.Active))
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Proyecto reactivado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<bool> PopulateTeamsAsync(ProjectFormViewModel model, bool lockTeam = false)
    {
        var profileId = await _currentUser.GetProfileIdAsync();
        if (profileId is null)
        {
            return false;
        }

        var teams = await _access.TeamsFor(profileId.Value)
            .OrderBy(t => t.Name)
            .Select(t => new
            {
                t.TeamId,
                t.Name,
                CanManage = t.Members.Any(m =>
                    m.ProfileId == profileId && (m.Role == TeamMemberRole.Owner || m.Role == TeamMemberRole.Admin))
            })
            .ToListAsync();

        var options = lockTeam
            ? teams.Where(t => t.TeamId == model.TeamId)
            : teams.Where(t => t.CanManage);

        model.Teams = options.Select(t => new SelectListItem(t.Name, t.TeamId.ToString(), t.TeamId == model.TeamId));
        return options.Any();
    }
}
