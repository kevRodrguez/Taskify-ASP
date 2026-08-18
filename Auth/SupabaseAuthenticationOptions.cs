using Microsoft.AspNetCore.Authentication;

namespace Taskify.Auth;

public class SupabaseAuthenticationOptions : AuthenticationSchemeOptions
{
    public PathString LoginPath { get; set; } = "/Auth/Login";

    public PathString AccessDeniedPath { get; set; } = "/Auth/Login";

    public string ReturnUrlParameter { get; set; } = "returnUrl";

    /// <summary>
    /// Margen con el que se refresca el access token antes de que caduque, para que
    /// una peticion no falle a mitad de camino.
    /// </summary>
    public TimeSpan RefreshMargin { get; set; } = TimeSpan.FromMinutes(1);
}
