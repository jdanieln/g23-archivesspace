using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ArchivesSpaceWeb.Domains.Identity.Entities;
using ArchivesSpaceWeb.Domains.Identity.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchivesSpaceWeb.Domains.Identity.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IRepository<Event> _eventRepository;

        public AccountController(IAuthService authService, IRepository<Event> eventRepository)
        {
            _authService = authService;
            _eventRepository = eventRepository;
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

            var user = await _authService.ValidateUserAsync(username, password, authMode);

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

                // US 37: Log login Event
                var logEvent = new Event
                {
                    EventType = "Login",
                    EventDate = DateTime.Now,
                    Description = $"Usuario '{user.Username}' inició sesión usando método '{user.AuthMode}' con rol '{user.Role}'."
                };
                await _eventRepository.AddAsync(logEvent);
                await _eventRepository.SaveChangesAsync();

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
            var username = User.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                // Log logout Event
                var logEvent = new Event
                {
                    EventType = "Logout",
                    EventDate = DateTime.Now,
                    Description = $"Usuario '{username}' cerró sesión."
                };
                await _eventRepository.AddAsync(logEvent);
                await _eventRepository.SaveChangesAsync();
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
