using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskify.ViewModels.Auth;
using Taskify.Services;
using static Supabase.Gotrue.Constants;

namespace Taskify.Controllers;

public class AuthController : Controller
{
    private const string StatusMessageKey = "StatusMessage";

    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return IsSignedIn ? RedirectToHome() : View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _auth.SignUpAsync(model.Email, model.Password, model.FullName);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        if (result.RequiresEmailConfirmation)
        {
            TempData[StatusMessageKey] =
                $"Te enviamos un correo a {model.Email}. Confirma tu cuenta desde ese enlace para poder entrar.";
            return RedirectToAction(nameof(Login));
        }

        return RedirectToHome();
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (IsSignedIn)
        {
            return RedirectToHome();
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _auth.SignInAsync(model.Email, model.Password);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        return RedirectToLocalOrHome(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _auth.SignOutAsync();
        return RedirectToHome();
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _auth.SendPasswordResetAsync(model.Email);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        // El mensaje no revela si el correo existe, para no permitir enumerar cuentas.
        TempData[StatusMessageKey] =
            "Si existe una cuenta con ese correo, te enviamos un enlace para restablecer la contraseña.";
        return RedirectToAction(nameof(Login));
    }

    /// <summary>
    /// Punto de entrada de los enlaces de correo. Las plantillas del dashboard apuntan
    /// aquí con el token_hash y el tipo de operación.
    /// </summary>
    [HttpGet("/auth/confirm")]
    public async Task<IActionResult> Confirm(
        [FromQuery(Name = "token_hash")] string? tokenHash,
        [FromQuery] string? type,
        [FromQuery] string? next)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            TempData[StatusMessageKey] = "El enlace no es válido. Solicita uno nuevo.";
            return RedirectToAction(nameof(Login));
        }

        var otpType = ParseOtpType(type);
        var result = await _auth.VerifyEmailTokenAsync(tokenHash, otpType);
        if (!result.Succeeded)
        {
            TempData[StatusMessageKey] = result.Error;
            return RedirectToAction(nameof(Login));
        }

        if (otpType == EmailOtpType.Recovery)
        {
            return RedirectToAction(nameof(ResetPassword));
        }

        TempData[StatusMessageKey] = "Tu cuenta quedó confirmada.";
        return RedirectToLocalOrHome(next);
    }

    /// <summary>
    /// Requiere sesión porque el enlace de recuperación ya la dejó activa al pasar
    /// por <see cref="Confirm"/>.
    /// </summary>
    [HttpGet]
    [Authorize]
    public IActionResult ResetPassword()
    {
        return View(new ResetPasswordViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _auth.UpdatePasswordAsync(model.Password);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        TempData[StatusMessageKey] = "Tu contraseña se actualizó correctamente.";
        return RedirectToHome();
    }

    [HttpGet]
    [Authorize]
    public IActionResult Profile()
    {
        return View();
    }

    private bool IsSignedIn => User.Identity?.IsAuthenticated ?? false;

    private IActionResult RedirectToHome() => RedirectToAction("Index", "Home");

    private IActionResult RedirectToLocalOrHome(string? url) =>
        !string.IsNullOrEmpty(url) && Url.IsLocalUrl(url) ? LocalRedirect(url) : RedirectToHome();

    private static EmailOtpType ParseOtpType(string? type) => type?.ToLowerInvariant() switch
    {
        "recovery" => EmailOtpType.Recovery,
        "invite" => EmailOtpType.Invite,
        "magiclink" => EmailOtpType.MagicLink,
        "email_change" => EmailOtpType.EmailChange,
        "signup" => EmailOtpType.Signup,
        _ => EmailOtpType.Email
    };
}
