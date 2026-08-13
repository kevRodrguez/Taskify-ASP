namespace Taskify.Models.Entities;

/// <summary>
/// Mapeo de solo lectura a auth.users (Supabase). La tabla la administra Supabase;
/// EF Core no genera migraciones para ella.
/// </summary>
public class AuthUser
{
    public Guid Id { get; set; }

    public Profile? Profile { get; set; }
}
