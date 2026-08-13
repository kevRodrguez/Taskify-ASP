using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskify.Authorization;
using Taskify.Models.Enums;
using Taskify.Services;
using Taskify.ViewModels.Tasks;

namespace Taskify.Controllers;

[Authorize]
public class TasksController : Controller
{
    private readonly ITaskService _tasks;
    private readonly IProjectService _projects;
    private readonly ITeamService _teams;

    public TasksController(ITaskService tasks, IProjectService projects, ITeamService teams)
    {
        _tasks = tasks;
        _projects = projects;
        _teams = teams;
    }

    [HttpGet]
    [TeamAuthorize("projectId", TeamAuthorizeResource.Project)]
    public async Task<IActionResult> Board(Guid projectId)
    {
        var project = await _projects.GetWithTeamAsync(projectId);
        if (project is null)
        {
            return NotFound();
        }

        var membership = (Models.Entities.TeamMember)HttpContext.Items[TeamAuthorizeFilter.MembershipItemKey]!;
        var cards = await _tasks.ListByProjectAsync(projectId);

        return View(new KanbanBoardViewModel
        {
            ProjectId = project.ProjectId,
            TeamId = project.TeamId,
            ProjectName = project.Name,
            TeamName = project.Team.Name,
            CanManage = TeamAccess.IsManager(membership.Role),
            Todo = cards.Where(c => c.Status == TaskItemStatus.Todo).ToList(),
            InProgress = cards.Where(c => c.Status == TaskItemStatus.InProgress).ToList(),
            Done = cards.Where(c => c.Status == TaskItemStatus.Done).ToList()
        });
    }

    [HttpGet]
    [TeamAuthorize("projectId", TeamAuthorizeResource.Project)]
    public async Task<IActionResult> Create(Guid projectId)
    {
        var project = await _projects.GetWithTeamAsync(projectId);
        if (project is null)
        {
            return NotFound();
        }

        var model = new TaskFormViewModel
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            Status = TaskItemStatus.Todo
        };
        await PopulateAssigneesAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [TeamAuthorize("projectId", TeamAuthorizeResource.Project)]
    public async Task<IActionResult> Create(Guid projectId, TaskFormViewModel model)
    {
        model.ProjectId = projectId;
        await ValidateAssigneeAsync(model);

        if (!ModelState.IsValid)
        {
            var project = await _projects.GetWithTeamAsync(projectId);
            model.ProjectName = project?.Name ?? string.Empty;
            await PopulateAssigneesAsync(model);
            return View(model);
        }

        await _tasks.CreateAsync(projectId, model);
        TempData["StatusMessage"] = "Tarea creada.";
        return RedirectToAction(nameof(Board), new { projectId });
    }

    [HttpGet]
    [TeamAuthorize(resource: TeamAuthorizeResource.Task)]
    public async Task<IActionResult> Edit(Guid id)
    {
        var task = await _tasks.GetAsync(id);
        if (task is null)
        {
            return NotFound();
        }

        var model = new TaskFormViewModel
        {
            TaskItemId = task.TaskItemId,
            ProjectId = task.ProjectId,
            ProjectName = task.Project.Name,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            DueDate = task.DueDate,
            AssignedToProfileId = task.AssignedToProfileId
        };
        await PopulateAssigneesAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [TeamAuthorize(resource: TeamAuthorizeResource.Task)]
    public async Task<IActionResult> Edit(Guid id, TaskFormViewModel model)
    {
        var task = await _tasks.GetAsync(id);
        if (task is null)
        {
            return NotFound();
        }

        model.TaskItemId = id;
        model.ProjectId = task.ProjectId;
        await ValidateAssigneeAsync(model);

        if (!ModelState.IsValid)
        {
            model.ProjectName = task.Project.Name;
            await PopulateAssigneesAsync(model);
            return View(model);
        }

        if (!await _tasks.UpdateAsync(id, model))
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Tarea actualizada.";
        return RedirectToAction(nameof(Board), new { projectId = task.ProjectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [TeamAuthorize(resource: TeamAuthorizeResource.Task)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var task = await _tasks.GetAsync(id);
        if (task is null)
        {
            return NotFound();
        }

        var projectId = task.ProjectId;
        if (!await _tasks.SoftDeleteAsync(id))
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Tarea eliminada.";
        return RedirectToAction(nameof(Board), new { projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [TeamAuthorize("projectId", TeamAuthorizeResource.Project)]
    public async Task<IActionResult> UpdateStatus(Guid projectId, [FromBody] UpdateTaskStatusViewModel model)
    {
        if (model.ProjectId != projectId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var task = await _tasks.UpdateStatusAsync(model.TaskItemId, model.Status, model.SortOrder);
        if (task is null || task.ProjectId != projectId)
        {
            return NotFound();
        }

        return Json(new
        {
            task.TaskItemId,
            task.ProjectId,
            task.Status,
            task.SortOrder,
            task.Title,
            task.Description,
            AssignedToName = task.AssignedTo?.FullName,
            task.DueDate,
            model.ClientRequestId
        });
    }

    private async Task PopulateAssigneesAsync(TaskFormViewModel model)
    {
        var project = await _projects.GetAsync(model.ProjectId);
        if (project is null)
        {
            model.Assignees = [];
            return;
        }

        var members = await _teams.ListMembersAsync(project.TeamId);
        var items = new List<SelectListItem>
        {
            new("Sin asignar", string.Empty, model.AssignedToProfileId is null)
        };
        items.AddRange(members.Select(m => new SelectListItem(
            $"{m.FullName} ({m.Email})",
            m.ProfileId.ToString(),
            m.ProfileId == model.AssignedToProfileId)));
        model.Assignees = items;
    }

    private async Task ValidateAssigneeAsync(TaskFormViewModel model)
    {
        if (model.AssignedToProfileId is not Guid assigneeId)
        {
            return;
        }

        if (!await _tasks.IsTeamMemberAssigneeAsync(model.ProjectId, assigneeId))
        {
            ModelState.AddModelError(nameof(model.AssignedToProfileId), "Solo puedes asignar la tarea a un miembro del equipo.");
        }
    }
}
