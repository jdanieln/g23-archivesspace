# ArchivesSpace G23 - Plataforma de Gestión Archivística

Bienvenido a **ArchivesSpace G23**, una plataforma web moderna para la catalogación, control, preservación y descripción de fondos documentales, colecciones históricas y registros de archivo. 

Este proyecto ha sido rediseñado bajo una arquitectura de **Domain-Driven Design (DDD) con Rebanadas Verticales (Vertical Slices)** para garantizar máxima modularidad, alta cohesión y bajo acoplamiento en el dominio del negocio.

---

## 🏛️ Arquitectura del Sistema: Diseño Dirigido por el Dominio (DDD)

Originalmente estructurado bajo un patrón MVC horizontal/por capas tradicionales (donde todos los controladores, entidades e interfaces residían en carpetas globales), el sistema fue refactorizado por completo hacia una estructura de **Contextos Acotados (Bounded Contexts)**.

Cada dominio de negocio es autónomo y autosuficiente. Contiene internamente todos los componentes necesarios para su funcionamiento técnico y de negocio.

### 📊 Estructura de Carpetas (Arquitectura de Dominios Verticales)

Toda la lógica de negocio y persistencia está agrupada bajo el directorio `Domains/`, subdividida en contextos claros:

```text
g23-archivesspace/
├── Domains/
│   ├── Accessions/              # Contexto de Nuevos Ingresos (Accessions)
│   │   ├── Controllers/
│   │   ├── Entities/
│   │   ├── Interfaces/
│   │   └── Infrastructure/
│   │
│   ├── Resources/               # Contexto de Recursos Archivísticos (Recursos, Jerarquías, EAD)
│   │   ├── Controllers/
│   │   ├── Entities/
│   │   ├── Interfaces/
│   │   └── Infrastructure/
│   │
│   ├── Agents/                  # Contexto de Agentes (Creadores, Personas, Entidades)
│   │   ├── Controllers/
│   │   ├── Entities/
│   │   ├── Interfaces/
│   │   └── Infrastructure/
│   │
│   ├── Identity/                # Contexto de Gestión de Identidad y Seguridad (Usuarios, Sesiones)
│   │   ├── Controllers/
│   │   ├── Entities/
│   │   ├── Interfaces/
│   │   └── Infrastructure/
│   │
│   ├── Admin/                   # Contexto de Administración, Auditorías y Panel de Control
│   │   ├── Controllers/
│   │   ├── Entities/
│   │   ├── Interfaces/
│   │   └── Infrastructure/
│   │
│   └── Shared/                  # Componentes Compartidos (DbContext común, Repositorio Genérico)
│       ├── Infrastructure/
│       └── Interfaces/
│
├── ViewModels/                  # Modelos de Vista específicos para la capa de presentación
├── Views/                       # Vistas de Razor (HTML y CSS Premium)
├── wwwroot/                     # Recursos estáticos (Imágenes, site.css, hierarchy.js)
└── Program.cs                   # Punto de entrada de la aplicación y registro de servicios
```

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
