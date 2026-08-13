using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskify.Services;
using Taskify.ViewModels.Notifications;

namespace Taskify.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public NotificationsController(ICurrentUserService currentUser, INotificationService notifications)
    {
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<IActionResult> Index()
    {
        var profileId = await _currentUser.GetProfileIdAsync();
        if (profileId is null)
        {
            return Forbid();
        }

        var items = await _notifications.ListForUserAsync(profileId.Value, 50);
        return View(items.Select(n => new NotificationListItemViewModel
        {
            NotificationId = n.NotificationId,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var profileId = await _currentUser.GetProfileIdAsync();
        if (profileId is null)
        {
            return Forbid();
        }

        await _notifications.MarkReadAsync(id, profileId.Value);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var profileId = await _currentUser.GetProfileIdAsync();
        if (profileId is null)
        {
            return Forbid();
        }

        await _notifications.MarkAllReadAsync(profileId.Value);
        TempData["StatusMessage"] = "Notificaciones marcadas como leídas.";
        return RedirectToAction(nameof(Index));
    }
}
