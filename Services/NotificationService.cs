using Microsoft.EntityFrameworkCore;
using Taskify.Data;
using Taskify.Models.Entities;

namespace Taskify.Services;

public sealed class NotificationService : INotificationService
{
    private readonly TaskifyDbContext _db;
    private readonly IEmailSender _email;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        TaskifyDbContext db,
        IEmailSender email,
        ILogger<NotificationService> logger)
    {
        _db = db;
        _email = email;
        _logger = logger;
    }

    public async Task NotifyTaskAssignedAsync(TaskItem task, Guid actorProfileId, CancellationToken cancellationToken = default)
    {
        if (task.AssignedToProfileId is not Guid assigneeId || assigneeId == actorProfileId)
        {
            return;
        }

        var assignee = task.AssignedTo
            ?? await _db.Profiles.FirstOrDefaultAsync(p => p.ProfileId == assigneeId, cancellationToken);
        if (assignee is null)
        {
            return;
        }

        var projectName = task.Project?.Name
            ?? await _db.Projects.Where(p => p.ProjectId == task.ProjectId).Select(p => p.Name).FirstOrDefaultAsync(cancellationToken)
            ?? "un proyecto";

        var message = $"Te asignaron la tarea «{task.Title}» en {projectName}.";
        await SaveInAppAsync(assigneeId, task.TaskItemId, message, cancellationToken);

        await TrySendAsync(
            assignee.Email,
            $"Nueva tarea asignada: {task.Title}",
            Wrap($"<p>Hola {System.Net.WebUtility.HtmlEncode(assignee.FullName)},</p>" +
                 $"<p>Te asignaron la tarea <strong>{System.Net.WebUtility.HtmlEncode(task.Title)}</strong> en el proyecto <strong>{System.Net.WebUtility.HtmlEncode(projectName)}</strong>.</p>" +
                 DueDateLine(task.DueDate)),
            cancellationToken);
    }

    public async Task<EmailDispatchResult> NotifyTaskCompletedAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        var project = task.Project
            ?? await _db.Projects.FirstOrDefaultAsync(p => p.ProjectId == task.ProjectId, cancellationToken);
        if (project is null)
        {
            return new EmailDispatchResult { SmtpConfigured = _email.IsConfigured };
        }

        var recipients = await _db.TeamMembers
            .Where(m => m.TeamId == project.TeamId)
            .Select(m => m.Profile)
            .ToListAsync(cancellationToken);

        if (task.AssignedToProfileId is Guid assigneeId
            && recipients.All(p => p.ProfileId != assigneeId))
        {
            var assignee = task.AssignedTo
                ?? await _db.Profiles.FirstOrDefaultAsync(p => p.ProfileId == assigneeId, cancellationToken);
            if (assignee is not null)
            {
                recipients.Add(assignee);
            }
        }

        recipients = recipients
            .Where(p => p is not null)
            .DistinctBy(p => p.ProfileId)
            .ToList();

        var message = $"La tarea «{task.Title}» se marcó como finalizada en {project.Name}.";
        var sent = 0;
        var failed = 0;

        foreach (var member in recipients)
        {
            await SaveInAppAsync(member.ProfileId, task.TaskItemId, message, cancellationToken);
            var outcome = await TrySendAsync(
                member.Email,
                $"Tarea finalizada: {task.Title}",
                Wrap($"<p>Hola {System.Net.WebUtility.HtmlEncode(member.FullName)},</p>" +
                     $"<p>La tarea <strong>{System.Net.WebUtility.HtmlEncode(task.Title)}</strong> del proyecto <strong>{System.Net.WebUtility.HtmlEncode(project.Name)}</strong> se marcó como finalizada.</p>"),
                cancellationToken);

            if (outcome == EmailSendOutcome.Sent)
            {
                sent++;
            }
            else if (outcome == EmailSendOutcome.Failed)
            {
                failed++;
            }
        }

        return new EmailDispatchResult
        {
            RecipientCount = recipients.Count,
            Sent = sent,
            Failed = failed,
            SmtpConfigured = _email.IsConfigured
        };
    }

    public async Task NotifyProjectCompletedAsync(Project project, CancellationToken cancellationToken = default)
    {
        var members = await _db.TeamMembers
            .Where(m => m.TeamId == project.TeamId)
            .Select(m => m.Profile)
            .ToListAsync(cancellationToken);

        var message = $"El proyecto «{project.Name}» se marcó como finalizado.";

        foreach (var member in members)
        {
            await SaveInAppAsync(member.ProfileId, null, message, cancellationToken);
            await TrySendAsync(
                member.Email,
                $"Proyecto finalizado: {project.Name}",
                Wrap($"<p>Hola {System.Net.WebUtility.HtmlEncode(member.FullName)},</p>" +
                     $"<p>El proyecto <strong>{System.Net.WebUtility.HtmlEncode(project.Name)}</strong> se marcó como finalizado.</p>"),
                cancellationToken);
        }
    }

    public async Task NotifyTaskDueSoonAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        if (task.AssignedToProfileId is not Guid assigneeId)
        {
            return;
        }

        var assignee = task.AssignedTo
            ?? await _db.Profiles.FirstOrDefaultAsync(p => p.ProfileId == assigneeId, cancellationToken);
        if (assignee is null)
        {
            return;
        }

        var due = task.DueDate?.ToString("dd/MM/yyyy") ?? "pronto";
        var message = $"La tarea «{task.Title}» vence el {due}.";
        await SaveInAppAsync(assigneeId, task.TaskItemId, message, cancellationToken);

        await TrySendAsync(
            assignee.Email,
            $"Tarea por vencer: {task.Title}",
            Wrap($"<p>Hola {System.Net.WebUtility.HtmlEncode(assignee.FullName)},</p>" +
                 $"<p>La tarea <strong>{System.Net.WebUtility.HtmlEncode(task.Title)}</strong> vence el <strong>{due}</strong>.</p>"),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> ListForUserAsync(Guid profileId, int take, CancellationToken cancellationToken = default)
    {
        return await _db.Notifications
            .Where(n => n.ProfileId == profileId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountUnreadAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        return _db.Notifications.CountAsync(n => n.ProfileId == profileId && !n.IsRead, cancellationToken);
    }

    public async Task MarkReadAsync(Guid notificationId, Guid profileId, CancellationToken cancellationToken = default)
    {
        var notification = await _db.Notifications.FirstOrDefaultAsync(
            n => n.NotificationId == notificationId && n.ProfileId == profileId,
            cancellationToken);
        if (notification is null || notification.IsRead)
        {
            return;
        }

        notification.IsRead = true;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllReadAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        await _db.Notifications
            .Where(n => n.ProfileId == profileId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);
    }

    private async Task SaveInAppAsync(Guid profileId, Guid? taskItemId, string message, CancellationToken cancellationToken)
    {
        _db.Notifications.Add(new Notification
        {
            ProfileId = profileId,
            TaskItemId = taskItemId,
            Message = message,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    private enum EmailSendOutcome
    {
        Sent,
        Failed,
        Skipped
    }

    private async Task<EmailSendOutcome> TrySendAsync(string to, string subject, string html, CancellationToken cancellationToken)
    {
        if (!_email.IsConfigured)
        {
            return EmailSendOutcome.Skipped;
        }

        try
        {
            await _email.SendAsync(to, subject, html, cancellationToken);
            return EmailSendOutcome.Sent;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo enviar el correo '{Subject}' a {To}.", subject, to);
            return EmailSendOutcome.Failed;
        }
    }

    private static string DueDateLine(DateOnly? dueDate)
    {
        return dueDate is { } date
            ? $"<p>Fecha de vencimiento: <strong>{date:dd/MM/yyyy}</strong>.</p>"
            : string.Empty;
    }

    private static string Wrap(string inner)
    {
        return $"""
            <div style="font-family:Inter,Arial,sans-serif;color:#1A1A1A;line-height:1.6">
              <p style="color:#EF661F;font-weight:600;letter-spacing:.08em;text-transform:uppercase;font-size:12px">Taskify</p>
              {inner}
              <p style="color:#707070;font-size:13px">Este mensaje es automático. No respondas a este correo.</p>
            </div>
            """;
    }
}
