using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using ArchivesSpaceWeb.Domains.Identity.Entities;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Agents.Entities;

namespace ArchivesSpaceWeb.Domains.Shared.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Repository> Repositories { get; set; } = null!;
        public DbSet<Resource> Resources { get; set; } = null!;
        public DbSet<ArchivalObject> ArchivalObjects { get; set; } = null!;
        public DbSet<Accession> Accessions { get; set; } = null!;
        public DbSet<Agent> Agents { get; set; } = null!;
        public DbSet<ResourceAgent> ResourceAgents { get; set; } = null!;
        public DbSet<AccessionAgent> AccessionAgents { get; set; } = null!;
        public DbSet<Subject> Subjects { get; set; } = null!;
        public DbSet<ResourceSubject> ResourceSubjects { get; set; } = null!;
        public DbSet<DigitalObject> DigitalObjects { get; set; } = null!;
        public DbSet<CollectionManagement> CollectionManagements { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<RightsStatement> RightsStatements { get; set; } = null!;
        public DbSet<Instance> Instances { get; set; } = null!;
        public DbSet<Container> Containers { get; set; } = null!;
        public DbSet<Location> Locations { get; set; } = null!;
        public DbSet<EnumList> EnumLists { get; set; } = null!;
        public DbSet<EnumValue> EnumValues { get; set; } = null!;
        public DbSet<ImportLog> ImportLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite Keys
            modelBuilder.Entity<ResourceAgent>()
                .HasKey(ra => new { ra.ResourceId, ra.AgentId });

            modelBuilder.Entity<AccessionAgent>()
                .HasKey(aa => new { aa.AccessionId, aa.AgentId });

            modelBuilder.Entity<ResourceSubject>()
                .HasKey(rs => new { rs.ResourceId, rs.SubjectId });

            // Self-referential hierarchy
            modelBuilder.Entity<ArchivalObject>()
                .HasOne(ao => ao.Parent)
                .WithMany()
                .HasForeignKey(ao => ao.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DigitalObject>()
                .HasOne(doObj => doObj.Parent)
                .WithMany()
                .HasForeignKey(doObj => doObj.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // 1. Repositories
            modelBuilder.Entity<Repository>().HasData(
                new Repository { Id = 1, Name = "Archivo Histórico Nacional", Code = "AHN", Description = "Repositorio principal de documentos históricos nacionales.", AdditionalPropertiesJson = "{\"Direccion\":\"Calle Principal 123\",\"Telefono\":\"+506 2222-3333\"}" },
                new Repository { Id = 2, Name = "Colecciones Especiales y Manuscritos", Code = "CEM", Description = "Colecciones de manuscritos raros, mapas y fotografías.", AdditionalPropertiesJson = "{\"Temperatura\":\"18°C\",\"Humedad\":\"45%\"}" }
            );

            // 2. Enums
            modelBuilder.Entity<EnumList>().HasData(
                new EnumList { Id = 1, Name = "level_of_description" },
                new EnumList { Id = 2, Name = "agent_role" },
                new EnumList { Id = 3, Name = "processing_status" }
            );

            modelBuilder.Entity<EnumValue>().HasData(
                // Levels of description
                new EnumValue { Id = 1, EnumListId = 1, Value = "Collection", Label = "Colección (Collection)" },
                new EnumValue { Id = 2, EnumListId = 1, Value = "Series", Label = "Serie (Series)" },
                new EnumValue { Id = 3, EnumListId = 1, Value = "Subseries", Label = "Subserie (Subseries)" },
                new EnumValue { Id = 4, EnumListId = 1, Value = "File", Label = "Expediente (File)" },
                new EnumValue { Id = 5, EnumListId = 1, Value = "Item", Label = "Documento único (Item)" },
                new EnumValue { Id = 6, EnumListId = 1, Value = "Other", Label = "Otro (Especificar)" },

                // Agent roles
                new EnumValue { Id = 7, EnumListId = 2, Value = "Creator", Label = "Creador" },
                new EnumValue { Id = 8, EnumListId = 2, Value = "Source", Label = "Procedencia (Source)" },
                new EnumValue { Id = 9, EnumListId = 2, Value = "Subject", Label = "Tema (Subject)" },

                // Processing Status
                new EnumValue { Id = 10, EnumListId = 3, Value = "New", Label = "Nuevo" },
                new EnumValue { Id = 11, EnumListId = 3, Value = "In Progress", Label = "En Progreso" },
                new EnumValue { Id = 12, EnumListId = 3, Value = "Completed", Label = "Completado" }
            );

            // 3. Users (Password hashes are SHA256 of "password123")
            string hash = HashPassword("password123");
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", PasswordHash = hash, Role = "SystemAdmin", AuthMode = "Local" },
                new User { Id = 2, Username = "archivista", PasswordHash = hash, Role = "RepositoryManager", AuthMode = "Local", RepositoryId = 1 },
                new User { Id = 3, Username = "entry", PasswordHash = hash, Role = "BasicDataEntry", AuthMode = "Local", RepositoryId = 1 },
                new User { Id = 4, Username = "read", PasswordHash = hash, Role = "ReadOnly", AuthMode = "Local", RepositoryId = 1 }
            );

            // 4. Subjects
            modelBuilder.Entity<Subject>().HasData(
                new Subject { Id = 1, Heading = "Historia de la Independencia", Source = "LCSH", StandardIdentifier = "sh85064892" },
                new Subject { Id = 2, Heading = "Archivos de la Administración Pública", Source = "FAST", StandardIdentifier = "fst00813350" },
                new Subject { Id = 3, Heading = "Guerra de la Frontera", Source = "LCSH", StandardIdentifier = "sh85015948" }
            );

            // 5. Agents
            modelBuilder.Entity<Agent>().HasData(
                new Agent { Id = 1, Type = "Person", Name = "Dr. Alejandro Flores Lara", Source = "local", AuthorityId = "auth-001" },
                new Agent { Id = 2, Type = "Corporate", Name = "Comisión Nacional de Historia", Source = "VIAF", AuthorityId = "viaf-12345678" },
                new Agent { Id = 3, Type = "Family", Name = "Familia Narváez Zelaya", Source = "local", AuthorityId = "auth-003" }
            );

            // 6. Resources (Collections)
            modelBuilder.Entity<Resource>().HasData(
                new Resource 
                { 
                    Id = 1, 
                    Title = "Fondo Documental Manuel Arce", 
                    Identifier = "HN-AHN-FMA-001", 
                    LevelOfDescription = "Collection",
                    Dates = "1821 - 1888", 
                    Extents = "12 Cajas Metálicas (approx. 4.5 metros lineales)",
                    LanguageOfDescription = "Español",
                    FindingAidAuthor = "Daniel Narváez",
                    FindingAidSponsor = "Ministerio de Cultura",
                    RepositoryId = 1,
                    SourceRecordId = "AT-REC-0099",
                    SourceSystem = "Archivists' Toolkit"
                },
                new Resource 
                { 
                    Id = 2, 
                    Title = "Colección de Mapas Cartográficos Coloniales", 
                    Identifier = "HN-CEM-MCC-002", 
                    LevelOfDescription = "Collection",
                    Dates = "1650 - 1799", 
                    Extents = "3 Planeros Especiales",
                    LanguageOfDescription = "Español y Latín",
                    FindingAidAuthor = "Sofía Altamirano",
                    FindingAidSponsor = "Fundación Histórica Americana",
                    RepositoryId = 2,
                    SourceRecordId = "ARCHON-334",
                    SourceSystem = "Archon"
                }
            );

            // 7. Archival Objects (Resource Components in hierarchy)
            modelBuilder.Entity<ArchivalObject>().HasData(
                new ArchivalObject { Id = 1, Title = "Correspondencia Oficial recibida de la Capitanía General", ComponentIdentifier = "HN-AHN-FMA-C01", LevelOfDescription = "Series", Dates = "1821 - 1830", Extents = "2 Cajas", ResourceId = 1, Position = 1 },
                new ArchivalObject { Id = 2, Title = "Correspondencia con Centroamérica", ComponentIdentifier = "HN-AHN-FMA-C01-S01", LevelOfDescription = "Subseries", Dates = "1821 - 1825", Extents = "1 Caja", ParentId = 1, ResourceId = 1, Position = 1 },
                new ArchivalObject { Id = 3, Title = "Acta de Proclamación de Febrero 1821", ComponentIdentifier = "HN-AHN-FMA-C01-S01-I01", LevelOfDescription = "Item", Dates = "15 de Febrero de 1821", Extents = "1 Folio", ParentId = 2, ResourceId = 1, Position = 1 },
                
                new ArchivalObject { Id = 4, Title = "Manuscritos Personales y Diarios Íntimos", ComponentIdentifier = "HN-AHN-FMA-C02", LevelOfDescription = "Series", Dates = "1840 - 1888", Extents = "1 Caja", ResourceId = 1, Position = 2 }
            );

            // 8. Accessions
            modelBuilder.Entity<Accession>().HasData(
                new Accession 
                { 
                    Id = 1, 
                    Title = "Donación Documentos de la Familia Flores", 
                    Identifier = "ACC-2026-001", 
                    AccessionDate = "2026-02-15", 
                    Dates = "1900 - 1950", 
                    Extents = "5 Cajas de cartón libre de ácido",
                    RepositoryId = 1,
                    SourceRecordId = "AT-ACC-1002",
                    SourceSystem = "Archivists' Toolkit"
                }
            );
        }

        public static string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (var b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
