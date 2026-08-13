namespace Taskify.ViewModels.Notifications;

public class NotificationListItemViewModel
{
    public Guid NotificationId { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
