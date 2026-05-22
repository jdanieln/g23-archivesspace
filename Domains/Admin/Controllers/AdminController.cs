using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Identity.Entities;
using ArchivesSpaceWeb.Domains.Identity.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ArchivesSpaceWeb.Domains.Admin.Controllers
{
    [Authorize(Roles = "SystemAdmin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IRepository<Repository> _repositoryRepo;
        private readonly IEnumService _enumService;
        private readonly IBackupService _backupService;

        public AdminController(
            IUserRepository userRepository,
            IRepository<Repository> repositoryRepo,
            IEnumService enumService,
            IBackupService backupService)
        {
            _userRepository = userRepository;
            _repositoryRepo = repositoryRepo;
            _enumService = enumService;
            _backupService = backupService;
        }

        // --- USER ACCOUNT MANAGEMENT --- (US 2, 25, 49)

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userRepository.GetUsersWithRepositoryAsync();
            ViewBag.Repositories = await _repositoryRepo.GetAllAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // US 25: Create a new User account
        public async Task<IActionResult> CreateUser(string username, string password, string role, int? repositoryId, string authMode)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "Usuario y Contraseña son obligatorios.";
                return RedirectToAction(nameof(Users));
            }

            var allUsers = await _userRepository.GetAllAsync();
            var existing = allUsers.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (existing)
            {
                TempData["ErrorMessage"] = "El nombre de usuario ya existe.";
                return RedirectToAction(nameof(Users));
            }

            var newUser = new User
            {
                Username = username,
                PasswordHash = ApplicationDbContext.HashPassword(password),
                Role = role,
                RepositoryId = repositoryId == 0 ? null : repositoryId,
                AuthMode = authMode
            };

            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Usuario '{username}' creado exitosamente.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // US 2: Reset User Password
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            if (string.IsNullOrEmpty(newPassword))
            {
                TempData["ErrorMessage"] = "La contraseña nueva no puede estar vacía.";
                return RedirectToAction(nameof(Users));
            }

            user.PasswordHash = ApplicationDbContext.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Contraseña del usuario '{user.Username}' restablecida.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // US 49: Declare that a user has the Repository Manager role for a given Repository
        public async Task<IActionResult> AssignRepoManager(int id, int repositoryId)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            if (repositoryId <= 0)
            {
                TempData["ErrorMessage"] = "Debes seleccionar un Repositorio válido.";
                return RedirectToAction(nameof(Users));
            }

            user.Role = "RepositoryManager";
            user.RepositoryId = repositoryId;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = $"El usuario '{user.Username}' ahora es Repository Manager del Repositorio seleccionado.";
            return RedirectToAction(nameof(Users));
        }

        // --- ENUMS LIST MANAGEMENT --- (US 3, 13, 17, 18)

        [HttpGet]
        public async Task<IActionResult> Enums()
        {
            var enums = await _enumService.GetEnumListsAsync();
            return View(enums);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEnumValue(int enumListId, string value, string label)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(label))
            {
                TempData["ErrorMessage"] = "Valor y Etiqueta son obligatorios.";
                return RedirectToAction(nameof(Enums));
            }

            try
            {
                await _enumService.AddEnumValueAsync(enumListId, value, label);
                TempData["SuccessMessage"] = $"Valor '{label}' agregado con éxito.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Enums));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // US 18: Bulk updating one enum value to another
        public async Task<IActionResult> BulkUpdateEnum(int enumListId, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(oldValue) || string.IsNullOrEmpty(newValue))
            {
                TempData["ErrorMessage"] = "Debes rellenar los valores antiguos y nuevos.";
                return RedirectToAction(nameof(Enums));
            }

            try
            {
                int affectedRows = await _enumService.BulkUpdateEnumValuesAsync(enumListId, oldValue, newValue);
                TempData["SuccessMessage"] = $"Actualización masiva completada. Se modificaron {affectedRows} registros de '{oldValue}' a '{newValue}'.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error durante la actualización masiva: {ex.Message}";
            }

            return RedirectToAction(nameof(Enums));
        }

        // --- DATABASE LOCATION & BACKUPS --- (US 30)

        [HttpGet]
        public IActionResult Database()
        {
            ViewBag.ConnectionString = _backupService.GetConnectionString();
            ViewBag.DatabasePath = _backupService.GetDatabasePath();
            ViewBag.DatabaseExists = _backupService.DatabaseExists();
            ViewBag.DatabaseSize = _backupService.GetDatabaseSize();
            ViewBag.LastModified = _backupService.GetLastModified();
            ViewBag.BackupFiles = _backupService.GetBackupFiles();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // US 30: Backup active database to /backups folder
        public async Task<IActionResult> BackupDatabase()
        {
            try
            {
                var destFileName = await _backupService.BackupDatabaseAsync();
                TempData["SuccessMessage"] = $"Copia de seguridad creada con éxito: '{destFileName}'";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error durante la copia de seguridad: {ex.Message}";
            }

            return RedirectToAction(nameof(Database));
        }
    }
}
