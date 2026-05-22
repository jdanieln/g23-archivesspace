using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ArchivesSpaceWeb.Domains.Identity.Entities;
using ArchivesSpaceWeb.Domains.Identity.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Infrastructure;

namespace ArchivesSpaceWeb.Domains.Identity.Infrastructure
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<User?> ValidateUserAsync(string username, string password, string authMode)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            if (authMode == "LDAP")
            {
                var ldapSettings = _configuration.GetSection("LdapSettings");
                bool isMock = ldapSettings.GetValue<bool>("MockEnabled");

                if (isMock)
                {
                    if (password == "ldap123" || password == "password123")
                    {
                        var user = await _context.Users
                            .Include(u => u.Repository)
                            .FirstOrDefaultAsync(u => u.Username == username && u.AuthMode == "LDAP");

                        if (user == null)
                        {
                            user = new User
                            {
                                Username = username,
                                Role = "ReadOnly", // Default role for new LDAP users
                                AuthMode = "LDAP",
                                PasswordHash = ApplicationDbContext.HashPassword(password)
                            };
                            _context.Users.Add(user);
                            await _context.SaveChangesAsync();
                        }
                        return user;
                    }
                }
                return null;
            }
            else
            {
                string hashedPassword = ApplicationDbContext.HashPassword(password);
                return await _context.Users
                    .Include(u => u.Repository)
                    .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hashedPassword && u.AuthMode == "Local");
            }
        }
    }
}
