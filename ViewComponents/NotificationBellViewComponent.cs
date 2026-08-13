using Microsoft.AspNetCore.Mvc;
using Taskify.Services;

namespace Taskify.ViewComponents;

public sealed class NotificationBellViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public NotificationBellViewComponent(ICurrentUserService currentUser, INotificationService notifications)
    {
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Content(string.Empty);
        }

        var profileId = await _currentUser.GetProfileIdAsync();
        var count = profileId is Guid id ? await _notifications.CountUnreadAsync(id) : 0;
        return View(count);
    }
}
