using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace Taskify.Auth;

/// <summary>
/// Persistencia de la sesion de Gotrue en una cookie cifrada. El SDK invoca estos
/// metodos por su cuenta a traves del PersistenceListener cuando el usuario inicia
/// sesion, se actualiza o rota el token, asi que la cookie se mantiene al dia sola.
/// </summary>
public class SupabaseCookieSessionPersistence : IGotrueSessionPersistence<Session>
{
    public const string CookieName = "sb-session";

    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);

    // Un access token de Supabase mas el objeto User pueden superar el limite de
    // 4096 bytes por cookie, asi que se reparte en trozos igual que hace el
    // esquema de cookies propio de ASP.NET Core.
    private static readonly ChunkingCookieManager CookieManager = new();

    private readonly IHttpContextAccessor _accessor;
    private readonly IDataProtector _protector;
    private readonly ILogger<SupabaseCookieSessionPersistence> _logger;

    public SupabaseCookieSessionPersistence(
        IHttpContextAccessor accessor,
        IDataProtectionProvider protectionProvider,
        ILogger<SupabaseCookieSessionPersistence> logger)
    {
        _accessor = accessor;
        _protector = protectionProvider.CreateProtector("Taskify.SupabaseSession.v1");
        _logger = logger;
    }

    public void SaveSession(Session session)
    {
        var context = _accessor.HttpContext;
        if (context is null || context.Response.HasStarted)
        {
            return;
        }

        try
        {
            var payload = _protector.Protect(JsonConvert.SerializeObject(session));
            CookieManager.AppendResponseCookie(context, CookieName, payload, BuildOptions(context));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo guardar la sesion de Supabase en la cookie.");
        }
    }

    public Session? LoadSession()
    {
        var context = _accessor.HttpContext;
        if (context is null)
        {
            return null;
        }

        var payload = CookieManager.GetRequestCookie(context, CookieName);
        if (string.IsNullOrEmpty(payload))
        {
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<Session>(_protector.Unprotect(payload));
        }
        catch (Exception ex) when (ex is CryptographicException or JsonException)
        {
            // Ocurre si el key ring cambio o la cookie fue manipulada.
            _logger.LogWarning(ex, "Cookie de sesion ilegible, se descarta.");
            DestroySession();
            return null;
        }
    }

    public void DestroySession()
    {
        var context = _accessor.HttpContext;
        if (context is null || context.Response.HasStarted)
        {
            return;
        }

        CookieManager.DeleteCookie(context, CookieName, BuildOptions(context));
    }

    private static CookieOptions BuildOptions(HttpContext context) => new()
    {
        HttpOnly = true,
        // En produccion Traefik termina el TLS, y UseForwardedHeaders hace que
        // IsHttps refleje el esquema real que vio el navegador.
        Secure = context.Request.IsHttps,
        // Lax permite que la cookie viaje cuando el usuario llega desde el enlace
        // del correo de confirmacion o recuperacion.
        SameSite = SameSiteMode.Lax,
        IsEssential = true,
        Path = "/",
        Expires = DateTimeOffset.UtcNow.Add(Lifetime)
    };
}
