using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ArchivesSpaceWeb.Domains.Identity.Entities;
using ArchivesSpaceWeb.Domains.Identity.Interfaces;
using ArchivesSpaceWeb.Domains.Identity.Commands;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchivesSpaceWeb.Domains.Identity.Controllers
{
    public class AccountController : Controller
    {
        private readonly ICommandHandler<LoginCommand, User?> _loginCommandHandler;
        private readonly ICommandHandler<LogoutCommand, bool> _logoutCommandHandler;

        public AccountController(
            ICommandHandler<LoginCommand, User?> loginCommandHandler,
            ICommandHandler<LogoutCommand, bool> logoutCommandHandler)
        {
            _loginCommandHandler = loginCommandHandler;
            _logoutCommandHandler = logoutCommandHandler;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string authMode, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "El usuario y la contraseña son requeridos.");
                return View();
            }

            var command = new LoginCommand(username, password, authMode);
            var user = await _loginCommandHandler.HandleAsync(command);

            if (user != null)
            {
                // Setup claims for identity mapping and authorization checks
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("AuthMode", user.AuthMode),
                    new Claim("RepositoryId", user.RepositoryId?.ToString() ?? "0")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError(string.Empty, "Nombre de usuario o contraseña incorrectos, o error de autenticación LDAP.");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            var command = new LogoutCommand(User.Identity?.Name);
            await _logoutCommandHandler.HandleAsync(command);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
