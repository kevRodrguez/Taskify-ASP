using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Taskify.Auth;
using static Supabase.Gotrue.Constants;
using static Supabase.Gotrue.Exceptions.FailureHint.Reason;

namespace Taskify.Services;

public class SupabaseAuthService : IAuthService
{
    private readonly Supabase.Client _supabase;
    private readonly SupabaseCookieSessionPersistence _persistence;
    private readonly ILogger<SupabaseAuthService> _logger;

    public SupabaseAuthService(
        Supabase.Client supabase,
        SupabaseCookieSessionPersistence persistence,
        ILogger<SupabaseAuthService> logger)
    {
        _supabase = supabase;
        _persistence = persistence;
        _logger = logger;
    }

    public async Task<SignUpResult> SignUpAsync(string email, string password, string? fullName)
    {
        var options = new SignUpOptions();
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            options.Data = new Dictionary<string, object> { ["full_name"] = fullName };
        }

        try
        {
            var session = await _supabase.Auth.SignUp(email, password, options);
            var user = session?.User ?? _supabase.Auth.CurrentUser;

            if (user?.Id is not { Length: > 0 } idString || !Guid.TryParse(idString, out var userId))
            {
                return new SignUpResult(false, false, "No se pudo registrar el usuario. Inténtalo de nuevo.");
            }

            // Con "Confirm email" activo, Supabase devuelve el usuario pero sin sesion.
            var confirmed = !string.IsNullOrEmpty(session?.AccessToken);
            return new SignUpResult(true, !confirmed, UserId: userId);
        }
        catch (GotrueException ex)
        {
            return new SignUpResult(false, false, Describe(ex));
        }
    }

    public async Task<AuthResult> SignInAsync(string email, string password)
    {
        try
        {
            var session = await _supabase.Auth.SignInWithPassword(email, password);

            return string.IsNullOrEmpty(session?.AccessToken)
                ? AuthResult.Failure("No se pudo iniciar sesión. Inténtalo de nuevo.")
                : AuthResult.Success();
        }
        catch (GotrueException ex)
        {
            return AuthResult.Failure(Describe(ex));
        }
    }

    public async Task SignOutAsync()
    {
        try
        {
            await _supabase.Auth.SignOut();
        }
        catch (Exception ex)
        {
            // Aunque Supabase no responda, la sesion local debe quedar cerrada.
            _logger.LogWarning(ex, "Fallo el cierre de sesion remoto.");
        }
        finally
        {
            _persistence.DestroySession();
        }
    }

    public async Task<AuthResult> SendPasswordResetAsync(string email)
    {
        try
        {
            await _supabase.Auth.ResetPasswordForEmail(email);
            return AuthResult.Success();
        }
        catch (GotrueException ex)
        {
            return AuthResult.Failure(Describe(ex));
        }
    }

    public async Task<AuthResult> VerifyEmailTokenAsync(string tokenHash, EmailOtpType type)
    {
        try
        {
            var session = await _supabase.Auth.VerifyTokenHash(tokenHash, type);

            return string.IsNullOrEmpty(session?.AccessToken)
                ? AuthResult.Failure("El enlace no es válido o ya fue utilizado.")
                : AuthResult.Success();
        }
        catch (GotrueException ex)
        {
            _logger.LogInformation(ex, "Enlace de correo rechazado por Supabase.");
            return AuthResult.Failure("El enlace expiró o ya fue utilizado. Solicita uno nuevo.");
        }
    }

    public async Task<AuthResult> UpdatePasswordAsync(string newPassword)
    {
        try
        {
            await _supabase.Auth.Update(new UserAttributes { Password = newPassword });
            return AuthResult.Success();
        }
        catch (GotrueException ex)
        {
            return AuthResult.Failure(Describe(ex));
        }
    }

    private static string Describe(GotrueException exception) => exception.Reason switch
    {
        UserBadLogin or UserBadMultiple => "Correo o contraseña incorrectos.",
        UserEmailNotConfirmed => "Debes confirmar tu correo antes de iniciar sesión. Revisa tu bandeja de entrada.",
        UserAlreadyRegistered => "Ya existe una cuenta registrada con ese correo.",
        UserBadEmailAddress => "El correo electrónico no es válido.",
        UserBadPassword => "La contraseña no cumple los requisitos mínimos del proyecto.",
        UserTooManyRequests => "Demasiados intentos seguidos. Espera unos minutos e inténtalo de nuevo.",
        Offline => "No hay conexión con el servidor de autenticación.",
        ExpiredRefreshToken or InvalidRefreshToken or NoSessionFound => "Tu sesión expiró. Vuelve a iniciar sesión.",
        _ => "No se pudo completar la operación. Inténtalo de nuevo."
    };
}
