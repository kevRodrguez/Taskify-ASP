namespace Taskify.Configuration;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;

    public string User { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    public bool UseSsl { get; set; } = true;

    /// <summary>Días de anticipación para el recordatorio de vencimiento.</summary>
    public int DueDateLookaheadDays { get; set; } = 1;
}
