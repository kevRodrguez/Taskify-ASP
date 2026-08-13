using Taskify.Models.Enums;

namespace Taskify.ViewModels.Tasks;

public class KanbanBoardViewModel
{
    public Guid ProjectId { get; set; }

    public Guid TeamId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string TeamName { get; set; } = string.Empty;

    public bool CanManage { get; set; }

    public IReadOnlyList<TaskCardViewModel> Todo { get; set; } = [];

    public IReadOnlyList<TaskCardViewModel> InProgress { get; set; } = [];

    public IReadOnlyList<TaskCardViewModel> Done { get; set; } = [];
}
