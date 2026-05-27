# ArchivesSpace G23 - Plataforma de Gestión Archivística

Bienvenido a **ArchivesSpace G23**, una plataforma web moderna para la catalogación, control, preservación y descripción de fondos documentales, colecciones históricas y registros de archivo. 

Este proyecto ha sido rediseñado bajo una arquitectura de **Domain-Driven Design (DDD) con Rebanadas Verticales (Vertical Slices)** para garantizar máxima modularidad, alta cohesión y bajo acoplamiento en el dominio del negocio.

---

## 🏛️ Arquitectura del Sistema: Diseño Dirigido por el Dominio (DDD) y CQRS Global

Originalmente estructurado bajo un patrón MVC horizontal/por capas tradicionales (donde todos los controladores, entidades e interfaces residían en carpetas globales), el sistema fue refactorizado por completo hacia una estructura de **Contextos Acotados (Bounded Contexts)** combinando **Domain-Driven Design (DDD)** con la segregación de responsabilidades mediante el patrón **CQRS** en rebanadas verticales (Vertical Slices).

Cada dominio de negocio es autónomo y autosuficiente. Contiene internamente todos los comandos, consultas, entidades, controladores e infraestructura necesarios para su funcionamiento técnico y de negocio.

### 📊 Estructura de Carpetas (Arquitectura de Dominios Verticales con CQRS)

Toda la lógica de negocio y persistencia está agrupada bajo el directorio `Domains/`, subdividida en contextos claros con segregación de comandos y consultas:

```text
g23-archivesspace/
├── Domains/
│   ├── Accessions/              # Contexto de Nuevos Ingresos (Accessions)
│   │   ├── Commands/            # Comandos de escritura (e.g. CreateAccession, ImportAccessionsCsv)
│   │   ├── Controllers/         # Controlador delgado (AccessionController)
│   │   ├── Entities/            # Entidades de dominio
│   │   ├── Infrastructure/      # Repositorios específicos, lógica persistente
│   │   ├── Interfaces/          # Abstracciones locales
│   │   └── Queries/             # Consultas de lectura (e.g. GetAccessionDetails)
│   │
│   ├── Resources/               # Contexto de Recursos Archivísticos (Recursos, Jerarquías, EAD)
│   │   ├── Commands/            # Comandos (e.g. CreateResource, EditResource, UpdateResourceHierarchy)
│   │   ├── Controllers/         # ResourceController, FindingAidController
│   │   ├── Entities/            # Resource, Component, DigitalObject, RightsStatement, etc.
│   │   ├── Infrastructure/      # EadExportService
│   │   ├── Interfaces/          # IEadExportService
│   │   └── Queries/             # Consultas (e.g. GetResourcesList, GetResourceDetails, ExportEad)
│   │
│   ├── Agents/                  # Contexto de Agentes (Creadores, Personas, Entidades)
│   │   ├── Commands/            # Comandos (e.g. CreateAgent, EditAgent)
│   │   ├── Controllers/         # AgentController
│   │   ├── Entities/            # Agent, AgentRelation, ContactRecord, etc.
│   │   ├── Infrastructure/      # EacCpfExportService
│   │   ├── Interfaces/          # IEacCpfExportService
│   │   └── Queries/             # Consultas (e.g. GetAgentsList, GetAgentDetails, ExportEacCpf)
│   │
│   ├── Identity/                # Contexto de Gestión de Identidad y Seguridad (Usuarios, Sesiones)
│   │   ├── Commands/            # Comandos (e.g. LoginCommand, LogoutCommand)
│   │   ├── Controllers/         # AccountController
│   │   ├── Entities/            # User, Repository, SessionLog
│   │   ├── Interfaces/          # IAuthService
│   │   └── Infrastructure/      # AuthService
│   │
│   ├── Admin/                   # Contexto de Administración, Auditorías y Panel de Control
│   │   ├── Commands/            # Comandos (e.g. CreateUser, ResetPassword, BackupDatabase, ImportEadXml)
│   │   ├── Controllers/         # AdminController, ImportController, DashboardController
│   │   ├── Entities/            # AuditLog, SystemSettings, ImportLog
│   │   ├── Infrastructure/      # AuditService
│   │   ├── Interfaces/          # IAuditService
│   │   └── Queries/             # Consultas (e.g. GetDashboardStats, SearchDashboard, GetDatabaseStats)
│   │
│   └── Shared/                  # Componentes Compartidos y Base de la Arquitectura
│       ├── Infrastructure/      # ApplicationDbContext, EFGenericRepository
│       └── Interfaces/          # ICommand, ICommandHandler, IQuery, IQueryHandler, IGenericRepository
│
├── ViewModels/                  # Modelos de Vista específicos para la capa de presentación
├── Views/                       # Vistas de Razor (HTML y CSS Premium)
├── wwwroot/                     # Recursos estáticos (Imágenes, site.css, hierarchy.js)
└── Program.cs                   # Punto de entrada de la aplicación y registro de servicios
```

---

## ⚡ Patrón CQRS & Principios SOLID (Refactorización Global)

El sistema ha sido completamente rediseñado bajo un enfoque riguroso de **Clean Code** y **Principios SOLID**, implementando una arquitectura de **CQRS (Command Query Responsibility Segregation)** hecha a la medida, libre de dependencias infladas.

### 📐 Principios SOLID Aplicados

1. **Single Responsibility Principle (SRP - Principio de Responsabilidad Única)**:
   * **Controladores Delgados**: Los controladores ya no gestionan lógica de negocio, validaciones pesadas, transacciones de base de datos ni auditorías. Ahora actúan únicamente como *despachadores minimalistas* que reciben peticiones HTTP y delegan la ejecución directamente a manejadores específicos de comandos (`ICommandHandler`) y consultas (`IQueryHandler`).
   * **Separación de Escrituras y Lecturas**: Cada caso de uso (como registrar un agente o exportar un árbol EAD) está aislado en su propia clase dedicada (ej. `CreateAgentCommand` o `ExportEadQuery`), garantizando que cada clase tenga un único motivo de cambio.

2. **Open/Closed Principle (OCP - Principio de Abierto/Cerrado)**:
   * El sistema está abierto a la extensión pero cerrado a la modificación. Para implementar un nuevo caso de uso (por ejemplo, un nuevo tipo de reporte o importación), basta con crear un nuevo `Command` o `Query` y su respectivo `Handler`. No hay necesidad de alterar los controladores existentes ni otras capas operativas del sistema.

3. **Liskov Substitution Principle (LSP - Principio de Sustitución de Liskov)**:
   * Se garantiza una interoperabilidad perfecta y coherencia de tipos mediante abstracciones genéricas. Cualquier manejador que implemente `IQueryHandler<TQuery, TResult>` o `ICommandHandler<TCommand, TResult>` puede sustituir de manera segura y transparente su comportamiento sin romper la lógica del despachador en el controlador.

4. **Interface Segregation Principle (ISP - Principio de Segregación de Interfaces)**:
   * Los controladores ya no dependen de interfaces de repositorio gigantescas ni de servicios monolíticos con docenas de métodos. En su lugar, dependen de interfaces ultra-enfocadas e individuales (`IQueryHandler<TQuery, TResult>` e `ICommandHandler<TCommand, TResult>`), que exponen un único método público: `HandleAsync`. Esto elimina dependencias innecesarias y reduce el acoplamiento drásticamente.

5. **Dependency Inversion Principle (DIP - Principio de Inversión de Dependencias)**:
   * Tanto los controladores como los manejadores dependen exclusivamente de abstracciones e interfaces, no de implementaciones de infraestructura concretas. Los servicios de persistencia, exportación y auditoría se inyectan a través del contenedor nativo de dependencias en `Program.cs`.

### 🛡️ Beneficios de la Arquitectura de CQRS Ligero (Custom-built)

* **Sin Acoplamiento a Terceros (No MediatR)**: En lugar de introducir paquetes NuGet externos como MediatR que añaden sobrecarga y complejidad, se construyó una infraestructura CQRS genérica y tipada nativa dentro de [Shared/Interfaces/](file:///Users/jdnarvaezf/Documents/Projects/g23-archivesspace/Domains/Shared/Interfaces/). Esto resulta en:
  * **Tipado Seguro en Tiempo de Compilación**: Errores en tipos de retorno o solicitudes se detectan inmediatamente al compilar, no en tiempo de ejecución.
  * **Máximo Rendimiento**: Cero sobrecarga de reflexión o enrutamiento de mensajes dinámicos pesados.
* **Control de Seguridad Nativo**: La lógica de autorización se extrajo de las acciones de los controladores y ahora se gestiona de forma elegante mediante atributos declarativos nativos de ASP.NET Core (`[Authorize(Roles = "...")]`), asegurando una capa de protección homogénea y legible.
* **Auditoría Centralizada**: Se integró un `IAuditService` unificado en lugar de invocaciones directas y repetitivas a repositorios de logs de base de datos desde los controladores, permitiendo auditorías consistentes y estructuradas ante cualquier operación de escritura en el sistema.

---

## 📐 Métricas de Arquitectura

Para asegurar la robustez de la plataforma, el rediseño se enfocó en optimizar las siguientes métricas:

### 1. Cohesión (Alta Cohesión Funcional)
* **Definición**: Grado en que los elementos dentro de un módulo pertenecen juntos.
* **Implementación**: Al encapsular `Controllers`, `Entities`, `Interfaces` e `Infrastructure` dentro de cada carpeta de dominio (por ejemplo, `Domains/Resources`), todos los cambios relacionados con un concepto de negocio se realizan en un único lugar, evitando la dispersión del código.

### 2. Acoplamiento (Bajo Acoplamiento)
* **Definición**: Nivel de interdependencia entre módulos.
* **Implementación**: Los dominios se comunican de forma minimalista. Las dependencias externas se resuelven a través de abstracciones (`Interfaces`) registradas mediante Inyección de Dependencias en `Program.cs`. El contexto `Shared` proporciona solo la infraestructura básica de persistencia (como `ApplicationDbContext` y `EFGenericRepository`), evitando dependencias directas o cruzadas entre dominios hermanos.

### 3. Granularidad del Dominio
* **Definición**: El tamaño y alcance en la subdivisión de las áreas de negocio.
* **Implementación**: Se definió una granularidad equilibrada. Conceptos complejos como **Resources** (que agrupan jerarquías de componentes, materias, objetos digitales y licencias de derechos) se mantuvieron juntos como un solo gran agregado debido a su fuerte cohesión de negocio. Por otro lado, la **Gestión de Identidad** e **Ingresos (Accessions)** se modelaron como subdominios independientes con límites claros.

---

## 🛠️ Tecnologías Utilizadas

* **Framework Principal**: .NET 8.0 (ASP.NET Core MVC).
* **Base de Datos y Persistencia**: Entity Framework Core con base de datos SQLite preconfigurada (`archives_space.db`).
* **Protección Concurrente**: Bloqueo Optimista (Optimistic Concurrency Control) implementado mediante campos `RowVersion` en los modelos clave para evitar sobrescrituras de datos.
* **Interfaz de Usuario**:
  * Razor Pages / Views con estilos personalizados premium en CSS Vanilla (con efectos de desenfoque, degradados elegantes y micro-animaciones).
  * Iconografía interactiva mediante **Lucide Icons**.
  * Organizador dinámico de jerarquía con soporte Drag & Drop y control por teclado accesible (`Views/Resource/EditHierarchy.cshtml` + `wwwroot/js/hierarchy.js`).

---

## 🚀 Instrucciones para Ejecutar el Proyecto

### Requisitos Previos
* **.NET 8 SDK** instalado en tu sistema.

### Construcción y Ejecución
1. Clona el repositorio si aún no lo has hecho:
   ```bash
   git clone git@github.com:jdanieln/g23-archivesspace.git
   ```
2. Navega al directorio del proyecto:
   ```bash
   cd g23-archivesspace
   ```
3. Restaura las dependencias y construye el proyecto:
   ```bash
   dotnet build
   ```
4. Ejecuta el servidor de desarrollo local:
   ```bash
   dotnet run
   ```
5. Abre tu navegador web en la dirección indicada en la terminal (usualmente `http://localhost:5000` o `https://localhost:5001`).

---

## 👨‍💻 Autores y Contribuciones

Este proyecto fue optimizado a una arquitectura limpia DDD por **Daniel Narváez** y la IA de pair programming **Antigravity (Google DeepMind)**. Licenciado bajo los términos de la Licencia MIT.
