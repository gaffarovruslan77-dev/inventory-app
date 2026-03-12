using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using AspNet.Security.OAuth.GitHub;
using InventoryApp.Web.Models;

namespace InventoryApp.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    // Кнопка "Войти через Google" — перенаправляет на Google
    public IActionResult LoginWithGoogle()
    {
        var redirectUrl = Url.Action("GoogleCallback", "Account");
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(
            GoogleDefaults.AuthenticationScheme, redirectUrl);
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    public IActionResult LoginWithGitHub()
    {
        var redirectUrl = Url.Action("GitHubCallback", "Account");
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(
            GitHubAuthenticationDefaults.AuthenticationScheme, redirectUrl);
        return Challenge(properties, GitHubAuthenticationDefaults.AuthenticationScheme);
    }

    // Google возвращает пользователя сюда после входа
    public async Task<IActionResult> GoogleCallback()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null) return RedirectToAction("Login");

        // Пробуем войти если пользователь уже есть
        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false);

        if (result.Succeeded)
        {
            var existingUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (existingUser?.IsBlocked == true)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login");
            }
            return RedirectToAction("Index", "Home");
        }

        // Если нет — создаём нового пользователя
        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
        var name = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? email;
        var avatar = info.Principal.FindFirst("picture")?.Value;

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            DisplayName = name,
            AvatarUrl = avatar,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user);
        if (createResult.Succeeded)
        {
            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> GitHubCallback()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null) return RedirectToAction("Login");

        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false);

        if (result.Succeeded)
        {
            var existingUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (existingUser?.IsBlocked == true)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login");
            }
            return RedirectToAction("Index", "Home");
        }

        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
        var name = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? email;

        // GitHub часто не отдаёт email без отдельного scope.
        if (string.IsNullOrWhiteSpace(email))
        {
            email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "github_user";
            email = $"{email}@github.local";
        }

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            DisplayName = string.IsNullOrWhiteSpace(name) ? email : name,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user);
        if (createResult.Succeeded)
        {
            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: false);
        }

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
