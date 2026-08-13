using Microsoft.EntityFrameworkCore;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Taskify.Data;
using Taskify.Models.Entities;

namespace Taskify.Services;

public class ProfileService : IProfileService
{
    private readonly TaskifyDbContext _db;
    private readonly Supabase.Client _supabase;

    public ProfileService(TaskifyDbContext db, Supabase.Client supabase)
    {
        _db = db;
        _supabase = supabase;
    }

    public async Task<Profile> GetOrCreateAsync(Guid userId, string fullName, string email)
    {
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
        var now = DateTimeOffset.UtcNow;

        if (profile is null)
        {
            profile = new Profile
            {
                UserId = userId,
                FullName = fullName.Trim(),
                Email = email.Trim(),
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.Profiles.Add(profile);
        }
        else
        {
            var changed = false;

            if (profile.FullName != fullName.Trim())
            {
                profile.FullName = fullName.Trim();
                changed = true;
            }

            if (profile.Email != email.Trim())
            {
                profile.Email = email.Trim();
                changed = true;
            }

            if (changed)
            {
                profile.UpdatedAt = now;
            }
        }

        await _db.SaveChangesAsync();
        return profile;
    }

    public async Task<Profile?> SyncFromSupabaseSessionAsync()
    {
        var user = _supabase.Auth.CurrentUser;
        if (user?.Id is not { Length: > 0 } idString || !Guid.TryParse(idString, out var userId))
        {
            return null;
        }

        var email = user.Email ?? string.Empty;
        var fullName = ExtractFullName(user, email);

        return await GetOrCreateAsync(userId, fullName, email);
    }

    public async Task<Profile?> GetByCurrentUserAsync()
    {
        return await SyncFromSupabaseSessionAsync();
    }

    public async Task<AuthResult> UpdateFullNameAsync(string fullName)
    {
        var user = _supabase.Auth.CurrentUser;
        if (user?.Id is not { Length: > 0 } idString || !Guid.TryParse(idString, out var userId))
        {
            return AuthResult.Failure("No hay una sesión activa.");
        }

        var trimmed = fullName.Trim();
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile is null)
        {
            return AuthResult.Failure("No se encontró tu perfil.");
        }

        try
        {
            await _supabase.Auth.Update(new UserAttributes
            {
                Data = new Dictionary<string, object> { ["full_name"] = trimmed }
            });
        }
        catch (GotrueException)
        {
            return AuthResult.Failure("No se pudo actualizar el nombre en la cuenta.");
        }

        profile.FullName = trimmed;
        profile.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();

        return AuthResult.Success();
    }

    private static string ExtractFullName(User user, string fallback)
    {
        if (user.UserMetadata?.TryGetValue("full_name", out var fullName) == true
            && fullName?.ToString() is { Length: > 0 } name)
        {
            return name;
        }

        return fallback;
    }
}
