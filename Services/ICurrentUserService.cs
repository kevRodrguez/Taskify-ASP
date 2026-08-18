using Taskify.Models.Entities;

namespace Taskify.Services;

/// <summary>
/// Perfil de dominio del usuario autenticado, cacheado por petición.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }

    Task<Profile?> GetProfileAsync(CancellationToken cancellationToken = default);

    Task<Guid?> GetProfileIdAsync(CancellationToken cancellationToken = default);
}
