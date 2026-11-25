using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Portal.Infrastructure.Identity;
using Portal.Web.Models;

namespace Portal.Web.Controllers;

public class AccountController(
    SignInManager<ApplicationUser> signIn,
    UserManager<ApplicationUser> users,
    ILogger<AccountController> log) : Controller
{
    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) =>
        User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Index", "Home")
            : View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await users.FindByEmailAsync(model.Email);
        if (user is { IsActive: true })
        {
            // lockoutOnFailure: true => 5 failures lock the account for 15 minutes (FR-04).
            var result = await signIn.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                log.LogInformation("User {UserId} signed in", user.Id);
                return LocalRedirectOrHome(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                log.LogWarning("User {UserId} locked out", user.Id);
                ModelState.AddModelError("", "Too many failed attempts. The account is locked for 15 minutes.");
                return View(model);
            }
        }

        // Generic message: never reveal whether the e-mail or the password was wrong.
        ModelState.AddModelError("", "Invalid e-mail or password.");
        return View(model);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    private IActionResult LocalRedirectOrHome(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) // blocks open-redirect attacks
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Home");
}
