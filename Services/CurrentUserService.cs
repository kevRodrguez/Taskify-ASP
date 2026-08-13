using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Taskify.Data;
using Taskify.Models.Entities;

namespace Taskify.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TaskifyDbContext _db;
    private Profile? _cached;
    private bool _loaded;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, TaskifyDbContext db)
    {
        _httpContextAccessor = httpContextAccessor;
        _db = db;
    }

    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public async Task<Profile?> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        if (_loaded)
        {
            return _cached;
        }

        _loaded = true;

        if (UserId is not Guid userId)
        {
            return null;
        }

        _cached = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        return _cached;
    }

    public async Task<Guid?> GetProfileIdAsync(CancellationToken cancellationToken = default)
    {
        var profile = await GetProfileAsync(cancellationToken);
        return profile?.ProfileId;
    }
}
