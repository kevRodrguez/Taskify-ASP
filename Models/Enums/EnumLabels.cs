namespace Taskify.Models.Enums;

public static class EnumLabels
{
    public static string Display(this TeamMemberRole role) => role switch
    {
        TeamMemberRole.Owner => "Propietario",
        TeamMemberRole.Admin => "Administrador",
        TeamMemberRole.Member => "Miembro",
        _ => role.ToString()
    };

    public static string Display(this ProjectStatus status) => status switch
    {
        ProjectStatus.Active => "Activo",
        ProjectStatus.Completed => "Finalizado",
        ProjectStatus.Archived => "Archivado",
        _ => status.ToString()
    };

    public static string Display(this TaskItemStatus status) => status switch
    {
        TaskItemStatus.Todo => "Por hacer",
        TaskItemStatus.InProgress => "En curso",
        TaskItemStatus.Done => "Finalizado",
        _ => status.ToString()
    };

    public static string BadgeClass(this ProjectStatus status) => status switch
    {
        ProjectStatus.Active => "badge-brand-info",
        ProjectStatus.Completed => "badge-brand-success",
        ProjectStatus.Archived => "badge-brand-muted",
        _ => "badge-brand-muted"
    };

    public static string BadgeClass(this TeamMemberRole role) => role switch
    {
        TeamMemberRole.Owner => "badge-brand-accent",
        TeamMemberRole.Admin => "badge-brand-info",
        _ => "badge-brand-muted"
    };
}
