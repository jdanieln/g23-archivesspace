using ArchivesSpaceWeb.Domains.Shared.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Infrastructure;
using ArchivesSpaceWeb.Domains.Identity.Interfaces;
using ArchivesSpaceWeb.Domains.Identity.Infrastructure;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Admin.Infrastructure;
using ArchivesSpaceWeb.Domains.Resources.Interfaces;
using ArchivesSpaceWeb.Domains.Resources.Infrastructure;
using ArchivesSpaceWeb.Domains.Accessions.Interfaces;
using ArchivesSpaceWeb.Domains.Accessions.Infrastructure;
using ArchivesSpaceWeb.Domains.Agents.Interfaces;
using ArchivesSpaceWeb.Domains.Agents.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// US 28: Use a configuration that is not prepackaged (appsettings.external.json)
var currentDirectory = Directory.GetCurrentDirectory();
builder.Configuration.AddJsonFile(Path.Combine(currentDirectory, "appsettings.external.json"), optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register DbContext with SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=archives_space.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Register Abstractions & Infrastructure Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(EFGenericRepository<>));
builder.Services.AddScoped<IUserRepository, EFUserRepository>();
builder.Services.AddScoped<IResourceRepository, EFResourceRepository>();
builder.Services.AddScoped<IAccessionRepository, EFAccessionRepository>();
builder.Services.AddScoped<IAgentRepository, EFAgentRepository>();

// Register Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEnumService, EnumService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IImportService, ImportService>();

// Add Cookie Authentication for Local and LDAP login (US 6, 54, 55)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

var app = builder.Build();

// Auto-migrate & seed SQLite database on startup (US 30)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Clean and create/seed database
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the SQLite database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication & Authorization (US 21 - Role management)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
