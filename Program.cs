using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Supabase;
using Taskify.Auth;
using Taskify.Configuration;
using Taskify.Data;
using Taskify.Services;
using Taskify.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IValidationAttributeAdapterProvider, TaskifyValidationAttributeAdapterProvider>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Falta la cadena de conexión. Configura ConnectionStrings:DefaultConnection " +
        "con dotnet user-secrets en desarrollo, o con variables de entorno en producción.");
}

builder.Services.AddDbContext<TaskifyDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<SupabaseSettings>(
    builder.Configuration.GetSection(SupabaseSettings.SectionName));
builder.Services.AddHttpContextAccessor();

// El key ring cifra la cookie de sesion. Sin una ruta persistente, cada redespliegue
// genera claves nuevas e invalida las sesiones y los tokens antiforgery de todos.
var dataProtection = builder.Services.AddDataProtection().SetApplicationName("Taskify");
var keyRingPath = builder.Configuration["DataProtection:KeyRingPath"];
if (!string.IsNullOrWhiteSpace(keyRingPath))
{
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keyRingPath));
}

builder.Services.AddScoped<SupabaseCookieSessionPersistence>();

// Scoped: el cliente guarda el JWT y el refresh token del usuario en su estado
// interno, asi que no puede compartirse entre peticiones.
builder.Services.AddScoped(sp =>
{
    var settings = sp.GetRequiredService<IOptions<SupabaseSettings>>().Value;

    if (string.IsNullOrWhiteSpace(settings.Url) || string.IsNullOrWhiteSpace(settings.AnonKey))
    {
        throw new InvalidOperationException(
            "Faltan las credenciales de Supabase. Configura Supabase:Url y Supabase:AnonKey " +
            "con dotnet user-secrets en desarrollo, o con variables de entorno en produccion.");
    }

    return new Client(settings.Url, settings.AnonKey, new SupabaseOptions
    {
        // true crearia un timer de refresco en segundo plano por cada peticion.
        AutoRefreshToken = false,
        AutoConnectRealtime = false,
        SessionHandler = sp.GetRequiredService<SupabaseCookieSessionPersistence>()
    });
});

builder.Services.AddScoped<IAuthService, SupabaseAuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITeamAccessService, TeamAccessService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services
    .AddAuthentication(SupabaseAuthenticationDefaults.Scheme)
    .AddScheme<SupabaseAuthenticationOptions, SupabaseAuthenticationHandler>(
        SupabaseAuthenticationDefaults.Scheme,
        options =>
        {
            options.LoginPath = "/Auth/Login";
            options.AccessDeniedPath = "/Home/AccessDenied";
        });

builder.Services.AddAuthorization();

var app = builder.Build();

// Traefik termina el TLS y habla HTTP con el contenedor. Sin esto la app cree que
// la peticion no es segura y UseHttpsRedirection entra en bucle.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
