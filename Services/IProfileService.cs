using Taskify.Models.Entities;

namespace Taskify.Services;

public interface IProfileService
{
    Task<Profile> GetOrCreateAsync(Guid userId, string fullName, string email);

    /// <summary>
    /// Crea o actualiza el perfil a partir de la sesión activa de Supabase.
    /// </summary>
    Task<Profile?> SyncFromSupabaseSessionAsync();

    Task<Profile?> GetByCurrentUserAsync();

    Task<AuthResult> UpdateFullNameAsync(string fullName);
}
