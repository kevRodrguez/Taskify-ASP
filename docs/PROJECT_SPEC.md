# Taskify — Especificación del proyecto

> Documento de contexto derivado del PDF de requisitos académico (*Taskify - Gestor de tareas*).
> Úsalo como referencia para implementación, revisiones y trabajo con agentes de IA.

## Metadatos

| Campo | Valor |
| --- | --- |
| **Institución** | Universidad Católica de El Salvador |
| **Carrera** | Ingeniería en Desarrollo de Software |
| **Materia** | Desarrollo de Aplicaciones Web |
| **Proyecto** | Taskify — App ASP de Manejo de Proyectos |
| **Docente** | Ing. Luis Enrique Vásquez |
| **Integrantes** | Kevin Fernando Rodríguez Posada (2023-RP-601), Escobar Preza Bryan Steven (2023-EP-603) |
| **Fecha del documento fuente** | 10 de agosto de 2026 |

---

## 1. Descripción general

Taskify es un **gestor de tareas y proyectos** orientado al trabajo colaborativo en equipos multidisciplinarios. La plataforma debe centralizar:

- Administración integral de **proyectos**
- Orquestación de **tareas**
- Conformación y gestión de **equipos de trabajo**
- Gestión de **usuarios**

### Stack técnico requerido (según especificación)

| Capa | Tecnología |
| --- | --- |
| Framework web | ASP.NET Core (MVC) |
| Persistencia | Entity Framework Core — **Code First** |
| Base de datos | Relacional (integridad referencial y validaciones estrictas) |
| Notificaciones en tiempo real | **WebSockets** |
| Notificaciones asíncronas | **Correo electrónico** |

El sistema debe informar de forma inmediata a los miembros del equipo sobre cambios de estado en las tareas asignadas, mediante un **sistema dual de notificaciones** (email + WebSockets).

---

## 2. Objetivos

### Objetivo general

Desarrollar una aplicación web integral para la gestión de proyectos y tareas, implementando ASP.NET Core, bases de datos relacionales y comunicaciones en tiempo real vía WebSocket para notificaciones, con el fin de optimizar el seguimiento del progreso y la colaboración **asíncrona y síncrona** dentro de equipos de trabajo.

### Objetivos específicos

1. **Modelo de datos relacional** — Estructurar una base de datos robusta con Entity Framework Core (Code First) que soporte operaciones de tareas y relaciones **One-to-Many** y **Many-to-Many** entre usuarios, equipos y proyectos.
2. **Tiempo real** — Implementar comunicación en tiempo real con WebSockets para reflejar cambios instantáneos en el estado de las tareas en el tablero principal, mejorando la reactividad de la UI.
3. **Notificaciones por email** — Integrar un módulo que alerte proactivamente sobre:
   - Asignación de nuevas tareas
   - Fechas de vencimiento inminentes
   - Modificaciones críticas en proyectos

---

## 3. Alcance funcional

El alcance cubre el **ciclo completo de gestión colaborativa**, organizado en los siguientes módulos:

### 3.1 Gestión de usuarios y equipos

- Registro, autenticación y gestión de perfiles de usuario
- Creación y administración de **equipos de trabajo**
- Asignación de múltiples usuarios a distintos equipos (**Many-to-Many**)

### 3.2 Administración de proyectos

- CRUD de proyectos (crear, editar, eliminar)
- Definición de fechas de inicio y plazos estimados
- Asignación de proyectos a equipos específicos

### 3.3 Gestor de tareas

- Creación de tareas de diversa naturaleza
- Asignación de tareas a miembros específicos del equipo
- Visualización en **tablero tipo Kanban**, agrupadas por estado, por ejemplo:
  - Por hacer
  - En curso
  - Finalizado

### 3.4 Sistema de notificaciones

| Canal | Comportamiento esperado |
| --- | --- |
| **WebSockets** | Actualización en tiempo real del tablero cuando un usuario modifica el estado de una tarea |
| **Email** | Envío automatizado al asignar tareas, al acercarse la fecha de vencimiento o al finalizar un proyecto |

### 3.5 Validación y seguridad

- Formularios protegidos con **Data Annotations** estrictas (cliente y servidor)
- Validación de coherencia de datos (p. ej. fecha de fin no anterior a fecha de inicio)

---

## 4. Modelo de dominio (inferido de la especificación)

Relaciones clave que el sistema debe soportar:

```
Usuario ──< M:N >── Equipo
Equipo  ──< 1:N >── Proyecto
Proyecto ──< 1:N >── Tarea
Tarea   ──> Usuario (asignado)
Tarea   ──> Estado (Kanban: Por hacer | En curso | Finalizado)
```

### Entidades principales (propuesta para implementación)

| Entidad | Responsabilidad |
| --- | --- |
| `Usuario` | Identidad, perfil, autenticación |
| `Equipo` | Agrupación colaborativa de usuarios |
| `Proyecto` | Contenedor con fechas, equipo asociado |
| `Tarea` | Unidad de trabajo con estado, asignado y vencimiento |
| `Notificación` | Registro de eventos notificables (opcional, para historial) |

---

## 5. Estado actual del repositorio

> Actualizado según la implementación existente en este repositorio ASP.NET Core.

### Implementado

| Área | Detalle |
| --- | --- |
| Autenticación | Registro, login, recuperación y restablecimiento de contraseña vía **Supabase Auth** |
| Sesiones | Cookie `sb-session` cifrada con Data Protection; handler personalizado `SupabaseAuthenticationHandler` |
| Vistas de auth | Login, Register, ForgotPassword, ResetPassword, Profile |
| Infraestructura | Docker, README de despliegue (Dokploy/VPS), user secrets |
| EF Core Code First | Entidades, Fluent API, migraciones PostgreSQL (Supabase) |
| Equipos | CRUD, miembros M:N por email, roles Owner/Admin/Member, soft delete |
| Proyectos | CRUD, fechas coherentes, estados Active/Completed/Archived |
| Tareas / Kanban | CRUD, tablero por estado, drag-and-drop, asignación a miembros |
| WebSockets | SignalR `TaskBoardHub`: el tablero se actualiza al cambiar el estado |
| Email de dominio | SMTP (MailKit): asignación, vencimiento inminente, proyecto finalizado |
| Notificaciones in-app | Historial por perfil y campana en la navbar |
| Validación | Data Annotations en ViewModels, `[DateNotBefore]`, `[DefinedEnum]`, CHECK en Postgres |

### Pendiente respecto a la especificación académica

Cubierto. Ver checklist en la sección 6.

### Nota sobre autenticación vs. especificación

La especificación asume un gestor ASP.NET clásico con EF Core. Este repositorio delega **identidad y credenciales** en Supabase, lo cual satisface el requisito de registro/autenticación pero implica:

- Los usuarios de negocio probablemente deban vincularse al `User.Id` / email de Supabase en tablas propias de EF Core
- Las notificaciones por email de tareas pueden implementarse aparte del flujo de auth de Supabase (SMTP, SendGrid, etc.)

---

## 6. Criterios de aceptación (checklist para IA y desarrolladores)

Usar esta lista para validar avances contra la especificación original:

- [x] Base de datos relacional creada con EF Core Code First y migraciones
- [x] Relación M:N Usuario ↔ Equipo funcional
- [x] Relación 1:N Equipo → Proyecto funcional
- [x] Relación 1:N Proyecto → Tarea funcional
- [x] CRUD completo de proyectos con fechas coherentes
- [x] CRUD de tareas con asignación a miembros del equipo
- [x] Tablero Kanban con estados configurables y drag-and-drop o cambio de estado
- [x] WebSockets: cambio de estado de tarea reflejado en tiempo real para otros clientes conectados
- [x] Email al asignar una tarea
- [x] Email al acercarse fecha de vencimiento
- [x] Email al finalizar un proyecto
- [x] Data Annotations en ViewModels/entidades con validación cliente y servidor
- [x] Reglas de negocio: fechas de fin ≥ fechas de inicio

---

## 7. Convenciones sugeridas para continuar el desarrollo

1. **Mantener MVC** — Controllers, Views, ViewModels/Models separados como en el código existente.
2. **EF Core en `Data/` o `Models/`** — DbContext, entidades de dominio y migraciones versionadas en git.
3. **Autorización** — Usar `[Authorize]` y claims de Supabase; extender con claims o tablas de membresía de equipo.
4. **Tiempo real** — SignalR es la opción idiomática en ASP.NET Core para WebSockets; hub dedicado para eventos de tareas.
5. **Kanban** — Vista por proyecto con columnas por estado; API o SignalR para mover tareas entre columnas.
6. **Emails de dominio** — Servicio `INotificationService` desacoplado del auth de Supabase.

---

## 8. Referencias en el repositorio

| Recurso | Ubicación |
| --- | --- |
| Guía de auth y despliegue | [`README.md`](../README.md) |
| Configuración Supabase | [`Configuration/SupabaseSettings.cs`](../Configuration/SupabaseSettings.cs) |
| Controlador de auth | [`Controllers/AuthController.cs`](../Controllers/AuthController.cs) |
| Servicio de auth | [`Services/SupabaseAuthService.cs`](../Services/SupabaseAuthService.cs) |

---

## 9. Texto fuente (PDF)

Contenido literal extraído del documento académico para trazabilidad:

> El proyecto consiste en el desarrollo de un moderno Gestor de Tareas y Proyectos diseñado para optimizar el trabajo colaborativo en equipos multidisciplinarios. Esta plataforma centralizada permitirá la administración integral de proyectos, orquestando tareas diversas, la conformación de equipos de trabajo y la gestión de usuarios. Construido bajo la robusta arquitectura MVC de ASP.NET Core y utilizando Entity Framework Core (Code First), el sistema garantiza la integridad relacional y validaciones estrictas. Para fomentar la inmediatez en la colaboración, el gestor integrará un sistema dual de notificaciones: alertas por correo electrónico y actualizaciones en tiempo real mediante WebSockets, informando instantáneamente a los miembros del equipo sobre cualquier cambio de estado en las tareas asignadas.
