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
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Queries;
using ArchivesSpaceWeb.Domains.Accessions.Commands;
using ArchivesSpaceWeb.Domains.Agents.Interfaces;
using ArchivesSpaceWeb.Domains.Agents.Infrastructure;
using ArchivesSpaceWeb.Domains.Agents.Entities;
using ArchivesSpaceWeb.Domains.Agents.Queries;
using ArchivesSpaceWeb.Domains.Agents.Commands;
using ArchivesSpaceWeb.Domains.Identity.Entities;
using ArchivesSpaceWeb.Domains.Identity.Commands;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Resources.Queries;
using ArchivesSpaceWeb.Domains.Resources.Commands;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Admin.Queries;
using ArchivesSpaceWeb.Domains.Admin.Commands;
using System.Xml.Linq;
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
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IEadExportService, EadExportService>();
builder.Services.AddScoped<IEacCpfExportService, EacCpfExportService>();
 
// Register Accession CQRS Handlers
builder.Services.AddScoped<IQueryHandler<GetAccessionsListQuery, List<Accession>>, GetAccessionsListQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetAccessionDetailsQuery, AccessionDetailsResult?>, GetAccessionDetailsQueryHandler>();
builder.Services.AddScoped<ICommandHandler<CreateAccessionCommand, Accession>, CreateAccessionCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ImportAccessionsCsvCommand, ImportAccessionsCsvResult>, ImportAccessionsCsvCommandHandler>();
 
// Register Identity CQRS Handlers
builder.Services.AddScoped<ICommandHandler<LoginCommand, User?>, LoginCommandHandler>();
builder.Services.AddScoped<ICommandHandler<LogoutCommand, bool>, LogoutCommandHandler>();
 
// Register Agents CQRS Handlers
builder.Services.AddScoped<IQueryHandler<GetAgentsListQuery, List<Agent>>, GetAgentsListQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetAgentDetailsQuery, AgentDetailsResult?>, GetAgentDetailsQueryHandler>();
builder.Services.AddScoped<IQueryHandler<ExportEacCpfQuery, XDocument?>, ExportEacCpfQueryHandler>();
builder.Services.AddScoped<ICommandHandler<CreateAgentCommand, Agent>, CreateAgentCommandHandler>();
builder.Services.AddScoped<ICommandHandler<EditAgentCommand, Agent>, EditAgentCommandHandler>();
 
// Register Resources CQRS Handlers
builder.Services.AddScoped<IQueryHandler<GetResourcesListQuery, List<Resource>>, GetResourcesListQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetResourceDetailsQuery, ResourceDetailsResult?>, GetResourceDetailsQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetResourceHierarchyQuery, List<ArchivalObject>>, GetResourceHierarchyQueryHandler>();
builder.Services.AddScoped<IQueryHandler<ExportEadQuery, XDocument?>, ExportEadQueryHandler>();
builder.Services.AddScoped<IQueryHandler<SearchFindingAidsQuery, List<Resource>>, SearchFindingAidsQueryHandler>();
builder.Services.AddScoped<ICommandHandler<CreateResourceCommand, Resource>, CreateResourceCommandHandler>();
builder.Services.AddScoped<ICommandHandler<EditResourceCommand, Resource>, EditResourceCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateResourceHierarchyCommand, bool>, UpdateResourceHierarchyCommandHandler>();
 
// Register Admin CQRS Handlers
builder.Services.AddScoped<IQueryHandler<GetDashboardStatsQuery, DashboardStatsResult>, GetDashboardStatsQueryHandler>();
builder.Services.AddScoped<IQueryHandler<SearchDashboardQuery, SearchDashboardResult>, SearchDashboardQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetImportLogsQuery, List<ImportLog>>, GetImportLogsQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetUsersListQuery, UsersListResult>, GetUsersListQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetEnumsListQuery, List<EnumList>>, GetEnumsListQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetDatabaseStatsQuery, DatabaseStatsResult>, GetDatabaseStatsQueryHandler>();
builder.Services.AddScoped<ICommandHandler<CreateUserCommand, bool>, CreateUserCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ResetPasswordCommand, bool>, ResetPasswordCommandHandler>();
builder.Services.AddScoped<ICommandHandler<AssignRepoManagerCommand, bool>, AssignRepoManagerCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CreateEnumValueCommand, bool>, CreateEnumValueCommandHandler>();
builder.Services.AddScoped<ICommandHandler<BulkUpdateEnumCommand, int>, BulkUpdateEnumCommandHandler>();
builder.Services.AddScoped<ICommandHandler<BackupDatabaseCommand, string>, BackupDatabaseCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ImportEadXmlCommand, ImportXmlResult>, ImportEadXmlCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ImportMarcXmlCommand, ImportXmlResult>, ImportMarcXmlCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ImportEacCpfXmlCommand, ImportXmlResult>, ImportEacCpfXmlCommandHandler>();

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
