using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using static Supabase.Gotrue.Exceptions.FailureHint.Reason;

namespace Taskify.Auth;

/// <summary>
/// Traduce la sesion de Supabase guardada en cookie a un ClaimsPrincipal de ASP.NET Core,
/// de modo que [Authorize] y User.Identity funcionen como con cualquier otro esquema.
/// </summary>
public class SupabaseAuthenticationHandler : AuthenticationHandler<SupabaseAuthenticationOptions>
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    private readonly Supabase.Client _supabase;
    private readonly SupabaseCookieSessionPersistence _persistence;

    public SupabaseAuthenticationHandler(
        IOptionsMonitor<SupabaseAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        Supabase.Client supabase,
        SupabaseCookieSessionPersistence persistence)
        : base(options, logger, encoder)
    {
        _supabase = supabase;
        _persistence = persistence;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        _supabase.Auth.LoadSession();

        var session = _supabase.Auth.CurrentSession;
        if (string.IsNullOrEmpty(session?.AccessToken))
        {
            return AuthenticateResult.NoResult();
        }

        if (IsExpiring(session.AccessToken))
        {
            try
            {
                // Rota el token y dispara TokenRefreshed, que reescribe la cookie.
                await _supabase.Auth.RefreshToken();
                session = _supabase.Auth.CurrentSession;
            }
            catch (GotrueException ex) when (ex.Reason is InvalidRefreshToken or ExpiredRefreshToken)
            {
                _persistence.DestroySession();
                return AuthenticateResult.NoResult();
            }
            catch (Exception ex)
            {
                // Un fallo de red no debe borrar la sesion: se reintenta en la
                // siguiente peticion y mientras tanto el usuario va como anonimo.
                Logger.LogWarning(ex, "No se pudo refrescar el token de Supabase.");
                return AuthenticateResult.NoResult();
            }

            if (string.IsNullOrEmpty(session?.AccessToken))
            {
                return AuthenticateResult.NoResult();
            }
        }

        var principal = BuildPrincipal(session);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var returnUrl = properties.RedirectUri
                        ?? $"{OriginalPathBase}{OriginalPath}{Request.QueryString}";

        var target = Options.LoginPath
                     + QueryString.Create(Options.ReturnUrlParameter, returnUrl);

        Response.Redirect(BuildRedirectUri(target));
        return Task.CompletedTask;
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.Redirect(BuildRedirectUri(Options.AccessDeniedPath));
        return Task.CompletedTask;
    }

    private bool IsExpiring(string accessToken)
    {
        try
        {
            return TokenHandler.ReadJwtToken(accessToken).ValidTo <= DateTime.UtcNow.Add(Options.RefreshMargin);
        }
        catch (Exception)
        {
            // Token ilegible: forzamos el camino de refresco para descartarlo.
            return true;
        }
    }

    /// <summary>
    /// Los claims salen del propio JWT. No se valida la firma porque el token llega
    /// de nuestra cookie cifrada con Data Protection: falsificarlo exigiria la clave
    /// del key ring, no basta con manipular el navegador.
    /// </summary>
    private ClaimsPrincipal BuildPrincipal(Session session)
    {
        var identity = new ClaimsIdentity(SupabaseAuthenticationDefaults.Scheme);
        var token = TokenHandler.ReadJwtToken(session.AccessToken);

        var userId = session.User?.Id ?? token.Subject;
        if (!string.IsNullOrEmpty(userId))
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        var email = session.User?.Email ?? token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        if (!string.IsNullOrEmpty(email))
        {
            identity.AddClaim(new Claim(ClaimTypes.Email, email));
            identity.AddClaim(new Claim(ClaimTypes.Name, email));
        }

        var role = token.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
        if (!string.IsNullOrEmpty(role))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        if (session.User?.UserMetadata is { } metadata
            && metadata.TryGetValue("full_name", out var fullName)
            && fullName?.ToString() is { Length: > 0 } name)
        {
            identity.AddClaim(new Claim(TaskifyClaimTypes.FullName, name));
        }

        return new ClaimsPrincipal(identity);
    }
}
