using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Admin.Queries;
using ArchivesSpaceWeb.Domains.Admin.Commands;
using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using System;
using System.Threading.Tasks;

namespace ArchivesSpaceWeb.Domains.Admin.Controllers
{
    [Authorize(Roles = "SystemAdmin")]
    public class AdminController : Controller
    {
        private readonly IQueryHandler<GetUsersListQuery, UsersListResult> _usersListQueryHandler;
        private readonly IQueryHandler<GetEnumsListQuery, System.Collections.Generic.List<EnumList>> _enumsQueryHandler;
        private readonly IQueryHandler<GetDatabaseStatsQuery, DatabaseStatsResult> _dbStatsQueryHandler;
        private readonly ICommandHandler<CreateUserCommand, bool> _createUserCommandHandler;
        private readonly ICommandHandler<ResetPasswordCommand, bool> _resetPasswordCommandHandler;
        private readonly ICommandHandler<AssignRepoManagerCommand, bool> _assignRepoManagerCommandHandler;
        private readonly ICommandHandler<CreateEnumValueCommand, bool> _createEnumValueCommandHandler;
        private readonly ICommandHandler<BulkUpdateEnumCommand, int> _bulkUpdateEnumCommandHandler;
        private readonly ICommandHandler<BackupDatabaseCommand, string> _backupDatabaseCommandHandler;

        public AdminController(
            IQueryHandler<GetUsersListQuery, UsersListResult> usersListQueryHandler,
            IQueryHandler<GetEnumsListQuery, System.Collections.Generic.List<EnumList>> enumsQueryHandler,
            IQueryHandler<GetDatabaseStatsQuery, DatabaseStatsResult> dbStatsQueryHandler,
            ICommandHandler<CreateUserCommand, bool> createUserCommandHandler,
            ICommandHandler<ResetPasswordCommand, bool> resetPasswordCommandHandler,
            ICommandHandler<AssignRepoManagerCommand, bool> assignRepoManagerCommandHandler,
            ICommandHandler<CreateEnumValueCommand, bool> createEnumValueCommandHandler,
            ICommandHandler<BulkUpdateEnumCommand, int> bulkUpdateEnumCommandHandler,
            ICommandHandler<BackupDatabaseCommand, string> backupDatabaseCommandHandler)
        {
            _usersListQueryHandler = usersListQueryHandler;
            _enumsQueryHandler = enumsQueryHandler;
            _dbStatsQueryHandler = dbStatsQueryHandler;
            _createUserCommandHandler = createUserCommandHandler;
            _resetPasswordCommandHandler = resetPasswordCommandHandler;
            _assignRepoManagerCommandHandler = assignRepoManagerCommandHandler;
            _createEnumValueCommandHandler = createEnumValueCommandHandler;
            _bulkUpdateEnumCommandHandler = bulkUpdateEnumCommandHandler;
            _backupDatabaseCommandHandler = backupDatabaseCommandHandler;
        }

        // --- USER ACCOUNT MANAGEMENT --- (US 2, 25, 49)

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var result = await _usersListQueryHandler.HandleAsync(new GetUsersListQuery());
            ViewBag.Repositories = result.Repositories;
            return View(result.Users);
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

            var result = await _usersListQueryHandler.HandleAsync(new GetUsersListQuery());
            var existing = System.Linq.Enumerable.Any(result.Users, u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (existing)
            {
                TempData["ErrorMessage"] = "El nombre de usuario ya existe.";
                return RedirectToAction(nameof(Users));
            }

            var command = new CreateUserCommand(username, password, role, repositoryId, authMode);
            await _createUserCommandHandler.HandleAsync(command);

            TempData["SuccessMessage"] = $"Usuario '{username}' creado exitosamente.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // US 2: Reset User Password
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                TempData["ErrorMessage"] = "La contraseña nueva no puede estar vacía.";
                return RedirectToAction(nameof(Users));
            }

            var command = new ResetPasswordCommand(id, newPassword);
            var success = await _resetPasswordCommandHandler.HandleAsync(command);

            if (!success) return NotFound();

            TempData["SuccessMessage"] = $"Contraseña restablecida.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // US 49: Declare that a user has the Repository Manager role for a given Repository
        public async Task<IActionResult> AssignRepoManager(int id, int repositoryId)
        {
            if (repositoryId <= 0)
            {
                TempData["ErrorMessage"] = "Debes seleccionar un Repositorio válido.";
                return RedirectToAction(nameof(Users));
            }

            var command = new AssignRepoManagerCommand(id, repositoryId);
            var success = await _assignRepoManagerCommandHandler.HandleAsync(command);

            if (!success) return NotFound();

            TempData["SuccessMessage"] = $"El usuario ahora es Repository Manager del Repositorio seleccionado.";
            return RedirectToAction(nameof(Users));
        }

        // --- ENUMS LIST MANAGEMENT --- (US 3, 13, 17, 18)

        [HttpGet]
        public async Task<IActionResult> Enums()
        {
            var enums = await _enumsQueryHandler.HandleAsync(new GetEnumsListQuery());
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
                var command = new CreateEnumValueCommand(enumListId, value, label);
                await _createEnumValueCommandHandler.HandleAsync(command);
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
                var command = new BulkUpdateEnumCommand(enumListId, oldValue, newValue);
                int affectedRows = await _bulkUpdateEnumCommandHandler.HandleAsync(command);
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
        public async Task<IActionResult> Database()
        {
            var result = await _dbStatsQueryHandler.HandleAsync(new GetDatabaseStatsQuery());
            ViewBag.ConnectionString = result.ConnectionString;
            ViewBag.DatabasePath = result.DatabasePath;
            ViewBag.DatabaseExists = result.DatabaseExists;
            ViewBag.DatabaseSize = result.DatabaseSize;
            ViewBag.LastModified = result.LastModified;
            ViewBag.BackupFiles = result.BackupFiles;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // US 30: Backup active database to /backups folder
        public async Task<IActionResult> BackupDatabase()
        {
            try
            {
                var command = new BackupDatabaseCommand();
                var destFileName = await _backupDatabaseCommandHandler.HandleAsync(command);
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
