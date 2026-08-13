namespace Taskify.Models.Entities;

public class Notification
{
    public Guid NotificationId { get; set; }

    public Guid ProfileId { get; set; }

    public Guid? TaskItemId { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Profile Profile { get; set; } = null!;

    public TaskItem? TaskItem { get; set; }
}
