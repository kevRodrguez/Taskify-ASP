using static Supabase.Gotrue.Constants;

namespace Taskify.Services;

public record AuthResult(bool Succeeded, string? Error = null)
{
    public static AuthResult Success() => new(true);

    public static AuthResult Failure(string error) => new(false, error);
}

/// <param name="RequiresEmailConfirmation">
/// Cierto cuando el proyecto tiene activado "Confirm email": Supabase crea el usuario
/// pero no devuelve sesion hasta que se confirma el correo.
/// </param>
public record SignUpResult(bool Succeeded, bool RequiresEmailConfirmation, string? Error = null);

public interface IAuthService
{
    Task<SignUpResult> SignUpAsync(string email, string password, string? fullName);

    Task<AuthResult> SignInAsync(string email, string password);

    Task SignOutAsync();

    Task<AuthResult> SendPasswordResetAsync(string email);

    Task<AuthResult> VerifyEmailTokenAsync(string tokenHash, EmailOtpType type);

    Task<AuthResult> UpdatePasswordAsync(string newPassword);
}
