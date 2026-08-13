using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Taskify.Configuration;
using Taskify.Data;
using Taskify.Models.Enums;

namespace Taskify.Services;

public sealed class DueDateReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DueDateReminderService> _logger;

    public DueDateReminderService(IServiceScopeFactory scopeFactory, ILogger<DueDateReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendRemindersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Falló el envío de recordatorios de vencimiento.");
            }

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }

    private async Task SendRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TaskifyDbContext>();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var settings = scope.ServiceProvider.GetRequiredService<IOptions<EmailSettings>>().Value;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var until = today.AddDays(Math.Max(1, settings.DueDateLookaheadDays));
        var startOfToday = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);

        var dueTasks = await db.TaskItems
            .Include(t => t.AssignedTo)
            .Include(t => t.Project)
            .Where(t => t.AssignedToProfileId != null
                        && t.DueDate != null
                        && t.DueDate >= today
                        && t.DueDate <= until
                        && t.Status != TaskItemStatus.Done
                        && (t.LastReminderSentAt == null || t.LastReminderSentAt < startOfToday))
            .ToListAsync(cancellationToken);

        foreach (var task in dueTasks)
        {
            await notifications.NotifyTaskDueSoonAsync(task, cancellationToken);
            task.LastReminderSentAt = DateTimeOffset.UtcNow;
        }

        if (dueTasks.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Se enviaron {Count} recordatorios de vencimiento.", dueTasks.Count);
        }
    }
}
