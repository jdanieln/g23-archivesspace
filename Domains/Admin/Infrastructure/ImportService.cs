using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using ArchivesSpaceWeb.Domains.Admin.Entities;
using ArchivesSpaceWeb.Domains.Admin.Interfaces;
using ArchivesSpaceWeb.Domains.Shared.Infrastructure;
using ArchivesSpaceWeb.Domains.Resources.Entities;
using ArchivesSpaceWeb.Domains.Accessions.Entities;
using ArchivesSpaceWeb.Domains.Agents.Entities;

namespace ArchivesSpaceWeb.Domains.Admin.Infrastructure
{
    public class ImportService : IImportService
    {
        private readonly ApplicationDbContext _context;

        public ImportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ImportLog>> GetRecentImportLogsAsync(int count)
        {
            return await _context.ImportLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Repository>> GetAllRepositoriesAsync()
        {
            return await _context.Repositories.ToListAsync();
        }

        public async Task<(bool Success, string Message, string LogDetails)> ImportEadAsync(Stream xmlStream, int repositoryId)
        {
            var logStream = new StringBuilder();
            try
            {
                XDocument doc = XDocument.Load(xmlStream);
                var root = doc.Root;
                if (root == null || root.Name.LocalName != "ead")
                {
                    throw new Exception("El archivo no contiene un nodo raíz '<ead>' válido.");
                }

                // 1. Parse Resource Title, ID, Dates, Extents
                var unittitle = root.Descendants().FirstOrDefault(d => d.Name.LocalName == "unittitle")?.Value ?? "Recurso EAD Importado";
                var unitid = root.Descendants().FirstOrDefault(d => d.Name.LocalName == "unitid")?.Value ?? $"EAD-{DateTime.Now.Ticks}";
                var unitdate = root.Descendants().FirstOrDefault(d => d.Name.LocalName == "unitdate")?.Value ?? "S.D.";
                var extent = root.Descendants().FirstOrDefault(d => d.Name.LocalName == "extent")?.Value ?? "1 Caja";
                var lang = root.Descendants().FirstOrDefault(d => d.Name.LocalName == "language")?.Value ?? "Español";

                var sourceRecordId = root.Descendants().FirstOrDefault(d => d.Name.LocalName == "eadid")?.Value ?? $"AT-{DateTime.Now.Ticks}";

                logStream.AppendLine($"Parseando Recurso principal: {unittitle} (ID: {unitid})");

                var resource = new Resource
                {
                    Title = unittitle,
                    Identifier = unitid,
                    Dates = unitdate,
                    Extents = extent,
                    LevelOfDescription = "Collection",
                    LanguageOfDescription = lang,
                    RepositoryId = repositoryId,
                    SourceRecordId = sourceRecordId,
                    SourceSystem = "EAD XML Import Engine"
                };

                _context.Resources.Add(resource);
                await _context.SaveChangesAsync();

                // 2. Parse nested components recursively
                var components = root.Descendants().Where(d => d.Name.LocalName == "c01" || d.Name.LocalName == "c");
                int componentCount = 0;
                
                foreach (var cNode in components)
                {
                    var cTitle = cNode.Element(cNode.Name.Namespace + "did")?.Element(cNode.Name.Namespace + "unittitle")?.Value 
                              ?? cNode.Descendants().FirstOrDefault(d => d.Name.LocalName == "unittitle")?.Value 
                              ?? "Componente Importado";
                              
                    var cId = cNode.Attribute("id")?.Value 
                           ?? cNode.Element(cNode.Name.Namespace + "did")?.Element(cNode.Name.Namespace + "unitid")?.Value 
                           ?? $"COMP-{DateTime.Now.Ticks}-{componentCount}";
                           
                    var cLevel = cNode.Attribute("level")?.Value ?? "Item";
                    var cDate = cNode.Descendants().FirstOrDefault(d => d.Name.LocalName == "unitdate")?.Value ?? "S.D.";
                    var cExtent = cNode.Descendants().FirstOrDefault(d => d.Name.LocalName == "extent")?.Value ?? string.Empty;

                    var archivalObject = new ArchivalObject
                    {
                        Title = cTitle,
                        ComponentIdentifier = cId,
                        LevelOfDescription = cLevel,
                        Dates = cDate,
                        Extents = cExtent,
                        ResourceId = resource.Id,
                        Position = componentCount++
                    };

                    _context.ArchivalObjects.Add(archivalObject);
                }

                await _context.SaveChangesAsync();

                // Log successfully
                var log = new ImportLog
                {
                    ImporterName = "EAD XML Importer",
                    Timestamp = DateTime.Now,
                    Status = "Success",
                    Details = $"EAD importado. Recurso: '{resource.Title}'. Componentes agregados: {componentCount}.",
                    ErrorLog = logStream.ToString()
                };
                _context.ImportLogs.Add(log);
                await _context.SaveChangesAsync();

                return (true, $"EAD importado con éxito. Se creó el recurso '{resource.Title}' con {componentCount} componentes.", logStream.ToString());
            }
            catch (Exception ex)
            {
                logStream.AppendLine($"ERROR CRÍTICO: {ex.Message}");
                logStream.AppendLine(ex.StackTrace);

                var log = new ImportLog
                {
                    ImporterName = "EAD XML Importer",
                    Timestamp = DateTime.Now,
                    Status = "Failed",
                    Details = "Falló la importación EAD XML.",
                    ErrorLog = logStream.ToString()
                };
                _context.ImportLogs.Add(log);
                await _context.SaveChangesAsync();

                return (false, "Error durante la importación. Revisa la bitácora de errores.", logStream.ToString());
            }
        }

        public async Task<(bool Success, string Message, string LogDetails)> ImportMarcXmlAsync(Stream xmlStream, int repositoryId, bool agentsAndSubjectsOnly)
        {
            var logStream = new StringBuilder();
            try
            {
                XDocument doc = XDocument.Load(xmlStream);
                var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
                
                // Read record tag
                var records = doc.Descendants(ns + "record").ToList();
                if (!records.Any() && doc.Root?.Name.LocalName == "record")
                {
                    records.Add(doc.Root);
                }

                if (!records.Any())
                {
                    throw new Exception("No se encontraron registros '<record>' de MARCXML.");
                }

                int resourcesCount = 0;
                int agentsCount = 0;
                int subjectsCount = 0;

                foreach (var rec in records)
                {
                    // 1. Get Title from field 245 subfield a
                    var field245 = rec.Descendants(ns + "datafield").FirstOrDefault(x => x.Attribute("tag")?.Value == "245");
                    var title = field245?.Descendants(ns + "subfield").FirstOrDefault(x => x.Attribute("code")?.Value == "a")?.Value ?? "Recurso MARCXML Sin Título";

                    // Get Identifier from field 001 or 099
                    var field001 = rec.Descendants(ns + "controlfield").FirstOrDefault(x => x.Attribute("tag")?.Value == "001")?.Value;
                    var identifier = field001 ?? $"MARC-{DateTime.Now.Ticks}";

                    // Get Extent from 300 subfield a
                    var field300 = rec.Descendants(ns + "datafield").FirstOrDefault(x => x.Attribute("tag")?.Value == "300");
                    var extents = field300?.Descendants(ns + "subfield").FirstOrDefault(x => x.Attribute("code")?.Value == "a")?.Value ?? "N/D";

                    // Get Date from 260 subfield c
                    var field260 = rec.Descendants(ns + "datafield").FirstOrDefault(x => x.Attribute("tag")?.Value == "260");
                    var date = field260?.Descendants(ns + "subfield").FirstOrDefault(x => x.Attribute("code")?.Value == "c")?.Value ?? "S.D.";

                    Resource? resource = null;
                    if (!agentsAndSubjectsOnly)
                    {
                        resource = new Resource
                        {
                            Title = title,
                            Identifier = identifier,
                            Dates = date,
                            Extents = extents,
                            LevelOfDescription = "Collection",
                            RepositoryId = repositoryId,
                            SourceRecordId = identifier,
                            SourceSystem = "MARCXML Loader"
                        };
                        _context.Resources.Add(resource);
                        resourcesCount++;
                        await _context.SaveChangesAsync();
                        logStream.AppendLine($"Creado Recurso desde MARC: '{title}'");
                    }

                    // 2. Extract Agents from 100 (Person) or 110 (Corporate)
                    var field100 = rec.Descendants(ns + "datafield").Where(x => x.Attribute("tag")?.Value == "100" || x.Attribute("tag")?.Value == "110");
                    foreach (var f100 in field100)
                    {
                        var agentName = f100.Descendants(ns + "subfield").FirstOrDefault(x => x.Attribute("code")?.Value == "a")?.Value;
                        var authorityId = f100.Descendants(ns + "subfield").FirstOrDefault(x => x.Attribute("code")?.Value == "0")?.Value;
                        
                        if (!string.IsNullOrEmpty(agentName))
                        {
                            var type = f100.Attribute("tag")?.Value == "100" ? "Person" : "Corporate";
                            var agent = new Agent
                            {
                                Name = agentName.TrimEnd(','),
                                Type = type,
                                Source = "MARCXML",
                                AuthorityId = authorityId ?? $"marc-{DateTime.Now.Ticks}"
                            };
                            _context.Agents.Add(agent);
                            agentsCount++;
                            await _context.SaveChangesAsync();
                            logStream.AppendLine($"Creado Agente: '{agent.Name}' (Autoridad: {agent.AuthorityId})");

                            if (resource != null)
                            {
                                _context.ResourceAgents.Add(new ResourceAgent
                                {
                                    ResourceId = resource.Id,
                                    AgentId = agent.Id,
                                    Role = "Creator"
                                });
                            }
                        }
                    }

                    // 3. Extract Subjects from 650
                    var field650 = rec.Descendants(ns + "datafield").Where(x => x.Attribute("tag")?.Value == "650");
                    foreach (var f650 in field650)
                    {
                        var heading = f650.Descendants(ns + "subfield").FirstOrDefault(x => x.Attribute("code")?.Value == "a")?.Value;
                        var subid = f650.Descendants(ns + "subfield").FirstOrDefault(x => x.Attribute("code")?.Value == "0")?.Value;

                        if (!string.IsNullOrEmpty(heading))
                        {
                            var subject = new Subject
                            {
                                Heading = heading.TrimEnd('.'),
                                Source = f650.Attribute("ind2")?.Value == "0" ? "LCSH" : "MARC",
                                StandardIdentifier = subid ?? $"sub-{DateTime.Now.Ticks}"
                            };
                            _context.Subjects.Add(subject);
                            subjectsCount++;
                            await _context.SaveChangesAsync();
                            logStream.AppendLine($"Creado Tema/Subject: '{subject.Heading}' (ID Estándar: {subject.StandardIdentifier})");

                            if (resource != null)
                            {
                                _context.ResourceSubjects.Add(new ResourceSubject
                                {
                                    ResourceId = resource.Id,
                                    SubjectId = subject.Id
                                });
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                var log = new ImportLog
                {
                    ImporterName = "MARCXML Importer",
                    Timestamp = DateTime.Now,
                    Status = "Success",
                    Details = $"MARCXML procesado. Recursos: {resourcesCount}, Agentes: {agentsCount}, Temas: {subjectsCount}.",
                    ErrorLog = logStream.ToString()
                };
                _context.ImportLogs.Add(log);
                await _context.SaveChangesAsync();

                return (true, $"MARCXML importado con éxito. Recursos: {resourcesCount}, Agentes: {agentsCount}, Temas: {subjectsCount} creados.", logStream.ToString());
            }
            catch (Exception ex)
            {
                logStream.AppendLine($"ERROR CRÍTICO: {ex.Message}");
                var log = new ImportLog
                {
                    ImporterName = "MARCXML Importer",
                    Timestamp = DateTime.Now,
                    Status = "Failed",
                    Details = "Falló la importación MARCXML.",
                    ErrorLog = logStream.ToString()
                };
                _context.ImportLogs.Add(log);
                await _context.SaveChangesAsync();

                return (false, "Error durante importación MARCXML. Revisa bitácora.", logStream.ToString());
            }
        }

        public async Task<(bool Success, string Message, string LogDetails)> ImportEacCpfAsync(Stream xmlStream)
        {
            var logStream = new StringBuilder();
            try
            {
                XDocument doc = XDocument.Load(xmlStream);
                var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
                
                var identityNode = doc.Descendants(ns + "identity").FirstOrDefault();
                if (identityNode == null)
                {
                    throw new Exception("No se encontró el nodo '<identity>' obligatorio en EAC-CPF.");
                }

                var entityType = identityNode.Element(ns + "entityType")?.Value ?? "person";
                var nameEntry = identityNode.Element(ns + "nameEntry");
                var namePart = nameEntry?.Element(ns + "part")?.Value ?? "Agente EAC-CPF Sin Nombre";
                var authorityId = doc.Descendants(ns + "recordId").FirstOrDefault()?.Value ?? $"eac-{DateTime.Now.Ticks}";

                logStream.AppendLine($"Parseando Agente: {namePart} ({entityType})");

                var agent = new Agent
                {
                    Name = namePart,
                    Type = entityType.ToLower() == "corporatebody" ? "Corporate" : (entityType.ToLower() == "family" ? "Family" : "Person"),
                    Source = "EAC-CPF Import",
                    AuthorityId = authorityId
                };

                _context.Agents.Add(agent);
                await _context.SaveChangesAsync();

                var log = new ImportLog
                {
                    ImporterName = "EAC-CPF Importer",
                    Timestamp = DateTime.Now,
                    Status = "Success",
                    Details = $"EAC-CPF procesado. Agente '{agent.Name}' importado con éxito.",
                    ErrorLog = logStream.ToString()
                };
                _context.ImportLogs.Add(log);
                await _context.SaveChangesAsync();

                return (true, $"Agente '{agent.Name}' ({agent.Type}) importado con éxito desde EAC-CPF.", logStream.ToString());
            }
            catch (Exception ex)
            {
                logStream.AppendLine($"ERROR CRÍTICO: {ex.Message}");
                var log = new ImportLog
                {
                    ImporterName = "EAC-CPF Importer",
                    Timestamp = DateTime.Now,
                    Status = "Failed",
                    Details = "Falló la importación EAC-CPF.",
                    ErrorLog = logStream.ToString()
                };
                _context.ImportLogs.Add(log);
                await _context.SaveChangesAsync();

                return (false, "Error durante importación EAC-CPF.", logStream.ToString());
            }
        }

        public async Task<(bool Success, int SuccessCount, int ErrorCount, string LogDetails)> ImportAccessionsCsvAsync(Stream csvStream, int repositoryId)
        {
            var logDetailsBuilder = new StringBuilder();
            int successCount = 0;
            int errorCount = 0;

            try
            {
                using (var reader = new StreamReader(csvStream, Encoding.UTF8))
                {
                    string? headerLine = await reader.ReadLineAsync();
                    if (headerLine == null)
                    {
                        throw new Exception("El archivo CSV está vacío.");
                    }

                    // Format check: title, identifier, accessionDate, dates, extents
                    var headers = headerLine.Split(',').Select(h => h.Trim().ToLower()).ToArray();
                    int titleIdx = Array.IndexOf(headers, "title");
                    int idIdx = Array.IndexOf(headers, "identifier");
                    int dateIdx = Array.IndexOf(headers, "accessiondate");
                    int datesIdx = Array.IndexOf(headers, "dates");
                    int extentsIdx = Array.IndexOf(headers, "extents");

                    if (titleIdx == -1 || idIdx == -1)
                    {
                        throw new Exception("Encabezados inválidos. Se requiere 'title' e 'identifier' en la cabecera.");
                    }

                    string? line;
                    int lineNum = 1;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lineNum++;
                        var tokens = line.Split(',');
                        if (tokens.Length < 2)
                        {
                            logDetailsBuilder.AppendLine($"Línea {lineNum}: Fila vacía o incompleta.");
                            errorCount++;
                            continue;
                        }

                        try
                        {
                            var title = tokens[titleIdx].Trim();
                            var identifier = tokens[idIdx].Trim();
                            var accessionDate = dateIdx != -1 && tokens.Length > dateIdx ? tokens[dateIdx].Trim() : DateTime.Now.ToString("yyyy-MM-dd");
                            var dates = datesIdx != -1 && tokens.Length > datesIdx ? tokens[datesIdx].Trim() : string.Empty;
                            var extents = extentsIdx != -1 && tokens.Length > extentsIdx ? tokens[extentsIdx].Trim() : string.Empty;

                            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(identifier))
                            {
                                logDetailsBuilder.AppendLine($"Línea {lineNum}: Título o Identificador faltante.");
                                errorCount++;
                                continue;
                            }

                            var accession = new Accession
                            {
                                Title = title,
                                Identifier = identifier,
                                AccessionDate = accessionDate,
                                Dates = dates,
                                Extents = extents,
                                RepositoryId = repositoryId,
                                SourceRecordId = $"CSV-{DateTime.Now.Ticks}",
                                SourceSystem = "CSV Bulk Importer"
                            };

                            _context.Accessions.Add(accession);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            logDetailsBuilder.AppendLine($"Línea {lineNum}: Error al parsear - {ex.Message}");
                            errorCount++;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // Save Import Log
                var log = new ImportLog
                {
                    ImporterName = "CSV Accessions",
                    Timestamp = DateTime.Now,
                    Status = errorCount == 0 ? "Success" : "Failed",
                    Details = $"CSV procesado. Éxito: {successCount}, Errores: {errorCount}.",
                    ErrorLog = logDetailsBuilder.ToString()
                };
                _context.ImportLogs.Add(log);
                await _context.SaveChangesAsync();

                return (true, successCount, errorCount, logDetailsBuilder.ToString());
            }
            catch (Exception ex)
            {
                var log = new ImportLog
                {
                    ImporterName = "CSV Accessions",
                    Timestamp = DateTime.Now,
                    Status = "Failed",
                    Details = "Error crítico durante importación de CSV.",
                    ErrorLog = ex.ToString()
                };
                _context.ImportLogs.Add(log);
                await _context.SaveChangesAsync();

                return (false, 0, 1, ex.Message);
            }
        }
    }
}
