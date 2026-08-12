namespace Taskify.Configuration;

public class SupabaseSettings
{
    public const string SectionName = "Supabase";

    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Clave publica del proyecto (anon / publishable). No es un secreto: lo que
    /// protege los datos son las politicas de RLS en Postgres.
    /// </summary>
    public string AnonKey { get; set; } = string.Empty;
}
